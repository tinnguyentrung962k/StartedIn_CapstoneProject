using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using System.Collections.Generic;
using System.Net;

namespace StartedIn.Service.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Contract> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ISignNowService _signNowService;
        private readonly IEmailService _emailService;
        private readonly IProjectRepository _projectRepository;
        private readonly IShareEquityRepository _shareEquityRepository;
        private readonly IDisbursementRepository _disbursementRepository;
        private readonly IAzureBlobService _azureBlobService;

        public ContractService(IContractRepository contractRepository,
            IUnitOfWork unitOfWork,
            ILogger<Contract> logger,
            UserManager<User> userManager,
            ISignNowService signNowService,
            IEmailService emailService,
            IProjectRepository projectRepository,
            IShareEquityRepository shareEquityRepository,
            IDisbursementRepository disbursementRepository, IAzureBlobService azureBlobService)
        {
            _contractRepository = contractRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _signNowService = signNowService;
            _emailService = emailService;
            _projectRepository = projectRepository;
            _shareEquityRepository = shareEquityRepository;
            _disbursementRepository = disbursementRepository;
            _azureBlobService = azureBlobService;
        }

        public async Task<Contract> CreateInvestmentContract(string userId, InvestmentContractCreateDTO investmentContractCreateDTO)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng");
            }
            var project = await _projectRepository.GetProjectById(investmentContractCreateDTO.Contract.ProjectId);
            if (project == null)
            {
                throw new NotFoundException("Không tìm thấy dự án");
            }
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, investmentContractCreateDTO.Contract.ProjectId);
            if (projectRole != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException("Bạn không phải nhóm trưởng");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var investor = await _userManager.FindByIdAsync(investmentContractCreateDTO.InvestorInfo.UserId);
                if (investor == null)
                {
                    throw new NotFoundException("Không tìm thấy nhà đầu tư");
                }
                Contract contract = new Contract
                {
                    ContractName = investmentContractCreateDTO.Contract.ContractName,
                    ContractPolicy = investmentContractCreateDTO.Contract.ContractPolicy,
                    ContractType = ContractTypeConstant.Investment,
                    CreatedBy = user.FullName,
                    ProjectId = investmentContractCreateDTO.Contract.ProjectId,
                    ContractStatus = ContractStatusConstant.Draft,
                    ContractIdNumber = investmentContractCreateDTO.Contract.ContractIdNumber
                };
                var leader = user;
                List<UserContract> usersInContract = new List<UserContract>();
                List<User> chosenUsersList = new List<User> { investor, leader };
                foreach (var chosenUser in chosenUsersList)
                {
                    UserContract userContract = new UserContract
                    {
                        ContractId = contract.Id,
                        UserId = chosenUser.Id
                    };
                    usersInContract.Add(userContract);
                }
                contract.UserContracts = usersInContract;
                ShareEquity shareEquity = new ShareEquity
                {
                    ContractId = contract.Id,
                    Contract = contract,
                    CreatedBy = user.FullName,
                    Percentage = investmentContractCreateDTO.InvestorInfo.Percentage,
                    StakeHolderType = RoleInTeamConstant.INVESTOR,
                    User = investor,
                    UserId = investor.Id,
                    ShareQuantity = project.TotalShares * investmentContractCreateDTO.InvestorInfo.ShareQuantity,
                };
                var contractEntity = _contractRepository.Add(contract);
                var shareEquityEntity = _shareEquityRepository.Add(shareEquity);
                var disbursementList = new List<Disbursement>();
                foreach (var disbursementTime in investmentContractCreateDTO.Disbursements)
                {
                    Disbursement disbursement = new Disbursement
                    {
                        Amount = disbursementTime.Amount,
                        Condition = disbursementTime.Condition,
                        Contract = contract,
                        ContractId = contract.Id,
                        CreatedBy = user.FullName,
                        DisbursementStatus = DisbursementStatusConstant.Pending,
                        StartDate = disbursementTime.StartDate,
                        EndDate = disbursementTime.EndDate,
                        Title = disbursementTime.Title,
                        Investor = investor,
                        InvestorId = investor.Id,
                        OrderCode = GenerateUniqueBookingCode(),
                        IsValidWithContract = false
                    };
                    disbursementList.Add(disbursement);
                    var disbursementEntity = _disbursementRepository.Add(disbursement);
                }
                ReplacePlaceHolder(contract, investor, leader, project, shareEquity, disbursementList,investmentContractCreateDTO.InvestorInfo.BuyPrice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return contractEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while creating the contract: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public long GenerateUniqueBookingCode()
        {
            var random = new Random();
            long orderCode;

            // Generate a random number with a desired number of digits, e.g., 6 digits
            orderCode = random.Next(100000, 999999);

            // Ensure the booking code is unique
            while (_disbursementRepository.ExistsAsync(orderCode).Result)
            {
                orderCode = random.Next(100000, 999999);
            }

            return orderCode;
        }

        public async Task<Contract> GetContractByContractId(string id)
        {
            var chosenContract = await _contractRepository.GetContractById(id);
            if (chosenContract == null)
            {
                throw new NotFoundException("Không có hợp đồng");
            }
            return chosenContract;
        }

        public async Task<IEnumerable<Contract>> GetContractsByUserIdInAProject(string userId, string projectId, int pageIndex, int pageSize)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng");
            }
            var chosenProject = await _projectRepository.GetOneAsync(projectId);
            if (chosenProject == null)
            {
                throw new NotFoundException("Không tìm thấy dự án");
            }
            var contracts = await _contractRepository.GetContractsByUserIdInAProject(userId, projectId, pageIndex, pageSize);
            if (contracts is null)
            {
                throw new NotFoundException("Không có hợp đồng nào trong danh sách");
            }
            return contracts;
        }

        public async Task<Contract> UploadContractFile(string userId, string contractId, IFormFile file)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng");
            }
            var chosenContract = await _contractRepository.GetContractById(contractId);
            if (chosenContract is null)
            {
                throw new NotFoundException("Hợp đồng không tìm thấy");
            }
            try
            {
                await _signNowService.AuthenticateAsync();
                var signNowDocumentId = await _signNowService.UploadDocumentAsync(file);
                var userPartys = new List<User>();
                foreach (var userInContract in chosenContract.UserContracts)
                {
                    User userParty = await _userManager.FindByIdAsync(userInContract.UserId);
                    userPartys.Add(userParty);
                }
                var userEmails = new List<string>();
                foreach (var userParty in userPartys)
                {
                    userEmails.Add(userParty.Email);
                }
                chosenContract.SignNowDocumentId = signNowDocumentId;
                chosenContract.LastUpdatedBy = user.FullName;
                chosenContract.LastUpdatedTime = DateTimeOffset.UtcNow;
                chosenContract.ContractStatus = ContractStatusConstant.Sent;
                var inviteResposne = await _signNowService.CreateFreeFormInvite(chosenContract.SignNowDocumentId, userEmails);
                _contractRepository.Update(chosenContract);
                await _unitOfWork.SaveChangesAsync();
                return chosenContract;
            }

            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while upload the contract file: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public async Task<Contract> ValidateContractOnSignedAsync(string id)
        {
            var chosenContract = await _contractRepository.GetContractById(id);
            if (chosenContract == null)
            {
                throw new NotFoundException("Không tìm thấy hợp đồng");
            }
            try
            {
                chosenContract.ValidDate = DateOnly.FromDateTime(DateTime.Now);
                chosenContract.ContractStatus = ContractStatusConstant.Completed;
                foreach (var equityShare in chosenContract.ShareEquities)
                {
                    equityShare.DateAssigned = DateOnly.FromDateTime(DateTime.Now);
                    _shareEquityRepository.Update(equityShare);
                }
                foreach (var disbursement in chosenContract.Disbursements)
                {
                    disbursement.IsValidWithContract = true;
                    _disbursementRepository.Update(disbursement);
                }
                _contractRepository.Update(chosenContract);
                await _unitOfWork.SaveChangesAsync();
                return chosenContract;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while update the contract: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public void ReplacePlaceHolder(Contract contract, User investor, User leader, Project project, ShareEquity shareEquity, List<Disbursement> disbursements, decimal? buyPrice)
        {
            // Path to your Word template
            string templatePath = @"C:\Users\Admin\Downloads\Mau-Hop-dong-mua-ban-co-phan.docx";

            // Dictionary of placeholders and their replacement values (excluding the table for now)
            var replacements = new Dictionary<string, string>
            {
                { "SOHOPDONG", contract.ContractIdNumber },
                { "CREATEDDATE", DateOnly.FromDateTime(DateTime.Now).ToString("dd-MM-yyyy") },
                { "NHADAUTU", investor.FullName },
                { "MAILNHADAUTU", investor.Email },
                { "SDTNDT", investor.PhoneNumber },
                { "CMNDNDT", investor.IdCardNumber },
                { "DCNDT", investor.Address },
                { "TENDUAN", project.ProjectName },
                { "MASOTHUE", project.CompanyIdNumber },
                { "SDTCDA", leader.PhoneNumber },
                { "CHUDUAN", leader.FullName },
                { "CMNDCDA", leader.IdCardNumber },
                { "MAIL", leader.Email },
                { "DCCDU", leader.Address },
                { "PHANTRAMCOPHAN", shareEquity.Percentage.ToString() },
                { "GIAMUA", buyPrice.ToString()},
                { "DIEUKHOANDUAN", contract.ContractPolicy },
            };

            // Define the output file path
            string outputFilePath = Path.Combine(Path.GetDirectoryName(templatePath), $"UpdateContract.docx");
            File.Copy(templatePath, outputFilePath, overwrite: true);

            // Open the document and perform the replacements
            using (WordprocessingDocument doc = WordprocessingDocument.Open(outputFilePath, true))
            {
                var body = doc.MainDocumentPart.Document.Body;

                // Replace text placeholders
                foreach (var paragraph in body.Descendants<Paragraph>())
                {
                    foreach (var run in paragraph.Descendants<Run>())
                    {
                        foreach (var text in run.Descendants<Text>())
                        {
                            foreach (var placeholder in replacements)
                            {
                                if (text.Text.Contains(placeholder.Key))
                                {
                                    text.Text = text.Text.Replace(placeholder.Key, placeholder.Value);
                                }
                            }
                        }
                    }
                }

                // Find the paragraph with "CACMOCGIAINGAN" placeholder
                var placeholderParagraph = body.Elements<Paragraph>().FirstOrDefault(p => p.InnerText.Contains("CACMOCGIAINGAN"));
                if (placeholderParagraph != null)
                {
                    // Insert the disbursement table after removing the placeholder paragraph
                    Table disbursementTable = CreateDisbursementTable(disbursements);
                    placeholderParagraph.InsertAfterSelf(disbursementTable);
                    placeholderParagraph.Remove();
                }

                doc.MainDocumentPart.Document.Save();
            }

            Console.WriteLine($"Document saved to: {outputFilePath}");
        }

        // Helper method to create a table for disbursements
        private Table CreateDisbursementTable(List<Disbursement> disbursements)
        {
            Table table = new Table();

            // Define table properties
            TableProperties tblProperties = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
                ));
            table.AppendChild(tblProperties);

            // Header row
            TableRow headerRow = new TableRow();
            headerRow.Append(
                CreateTableCell("Tên cột mốc giải ngân", true),
                CreateTableCell("Số tiền", true),
                CreateTableCell("Ngày bắt đầu", true),
                CreateTableCell("Ngày hạn chót", true),
                CreateTableCell("Điều kiện", true)
            );
            table.AppendChild(headerRow);

            // Data rows
            foreach (var d in disbursements)
            {
                TableRow row = new TableRow();
                row.Append(
                    CreateTableCell(d.Title),
                    CreateTableCell(d.Amount.ToString("N3")),
                    CreateTableCell(d.StartDate.ToString("dd-MM-yyyy")),
                    CreateTableCell(d.EndDate.ToString("dd-MM-yyyy")),
                    CreateTableCell(d.Condition)
                );
                table.AppendChild(row);
            }

            return table;
        }

        // Helper method to create a table cell with text
        private TableCell CreateTableCell(string text, bool isHeader = false)
        {
            TableCell cell = new TableCell();
            var paragraph = new Paragraph(new Run(new Text(text)));

            // Set header style if it’s a header cell
            if (isHeader)
            {
                RunProperties runProperties = new RunProperties(new Bold());
                paragraph.Descendants<Run>().First().PrependChild(runProperties);
            }

            cell.Append(paragraph);
            return cell;
        }
    }
}
