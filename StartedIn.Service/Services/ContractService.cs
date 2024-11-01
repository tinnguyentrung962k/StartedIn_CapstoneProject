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
            var projectRole = await _projectRepository.GetUserRoleInProject(userId,investmentContractCreateDTO.Contract.ProjectId);
            if (projectRole != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException("Bạn không phải nhóm trưởng");
            }
            try {
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
                ReplacePlaceHolder(contract, investor, leader, project, shareEquity, disbursementList);
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
            var contracts = await _contractRepository.GetContractsByUserIdInAProject(userId,projectId,pageIndex,pageSize);
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

            catch (Exception ex) {
                _logger.LogError($"An error occurred while upload the contract file: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }   
        }
        public async Task<Contract> ValidateContractOnSignedAsync(string id)
        {
            var chosenContract = await _contractRepository.GetContractById(id);
            if (chosenContract == null) {
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
        public void ReplacePlaceHolder(Contract contract, User investor, User leader, Project project, ShareEquity shareEquity,List<Disbursement> disbursements)
        {
            string templatePath = @"C:\Users\Admin\Downloads\Mau-Hop-dong-mua-ban-co-phan.docx";

            string disbursementsText = string.Join("\n\n", disbursements.Select(d =>
                    $"{d.Title}:\n" +
                    $"    Số tiền: {d.Amount:N0}\n" + // Sử dụng định dạng số với dấu phân cách
                    $"    Từ ngày: {d.StartDate:dd-MM-yyyy}\n" +
                    $"    Đến ngày: {d.EndDate:dd-MM-yyyy}\n" +
                    $"    Điều kiện: {d.Condition}."
            ));

            var replacements = new Dictionary<string, string>
            {
                { "CREATEDDATE",DateOnly.FromDateTime(DateTime.Now).ToString("dd-MM-yyyy")},
                { "NHADAUTU",investor.FullName},
                { "MAILNHADAUTU",investor.Email},
                { "SDTNDT",investor.PhoneNumber},
                { "CMNDNDT",""},
                { "DCNDT",""},
                { "TENDUAN",project.ProjectName},
                { "MASOTHUE",""},
                { "SDTCDA",leader.PhoneNumber},
                { "CHUDUAN",leader.FullName},
                { "CMNDCDA",""},
                { "MAIL",leader.Email},
                { "DCCDU",""},
                { "PHANTRAMCOPHAN",shareEquity.Percentage.ToString()},
                { "GIAMUA",""},
                { "CACMOCGIAINGAN",disbursementsText},
                { "DIEUKHOANDUAN",contract.ContractPolicy},
            };
            string outputFilePath = Path.Combine(Path.GetDirectoryName(templatePath), "UpdatedContract.docx");
            // Tạo file mới với nội dung đã được thay thế
            File.Copy(templatePath, outputFilePath);

            using (WordprocessingDocument doc = WordprocessingDocument.Open(outputFilePath, true))
            {
                var body = doc.MainDocumentPart.Document.Body;
                foreach (var text in body.Descendants<Text>())
                {
                    Console.WriteLine(text.InnerText);
                    foreach (var placeholder in replacements)
                    {
                        if (text.Text.Equals(placeholder.Key))
                        {
                            
                            Console.WriteLine($"Replacing '{placeholder.Key}' with '{placeholder.Value}'"); // Ghi lại quá trình thay thế
                            text.Text = text.Text.Replace(placeholder.Key, placeholder.Value);
                        }
                    }
                }
                doc.MainDocumentPart.Document.Save();
            }

            Console.WriteLine($"Document saved to: {outputFilePath}");
        }
    }
}
