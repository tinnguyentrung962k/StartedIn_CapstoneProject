using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.SignNowWebhookRequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using StartedIn.CrossCutting.Enum;
using System.Collections.Generic;
using System.Net;
using StartedIn.CrossCutting.Enum;

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
        private readonly IDocumentFormatService _documentFormatService;
        private readonly IConfiguration _configuration;
        private readonly string _apiDomain;
        private readonly IUserService _userService;

        public ContractService(IContractRepository contractRepository,
            IUnitOfWork unitOfWork,
            ILogger<Contract> logger,
            UserManager<User> userManager,
            ISignNowService signNowService,
            IEmailService emailService,
            IProjectRepository projectRepository,
            IShareEquityRepository shareEquityRepository,
            IDisbursementRepository disbursementRepository, IAzureBlobService azureBlobService, 
            IDocumentFormatService documentFormatService,IConfiguration configuration,
            IUserService userService)
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
            _documentFormatService = documentFormatService;
            _configuration = configuration;
            _userService = userService;
            _apiDomain = _configuration.GetValue<string>("API_DOMAIN") ?? _configuration["Local_domain"];
        }
        

        public async Task<Contract> CreateInvestmentContract(string userId, InvestmentContractCreateDTO investmentContractCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, investmentContractCreateDTO.Contract.ProjectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, investmentContractCreateDTO.Contract.ProjectId);
            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
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
                    ContractType = ContractTypeEnum.INVESTMENT,
                    CreatedBy = userInProject.User.FullName,
                    ProjectId = investmentContractCreateDTO.Contract.ProjectId,
                    ContractStatus = ContractStatusEnum.DRAFT,
                    ContractIdNumber = investmentContractCreateDTO.Contract.ContractIdNumber
                };
                var leader = userInProject.User;
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
                    CreatedBy = userInProject.User.FullName,
                    Percentage = investmentContractCreateDTO.InvestorInfo.Percentage,
                    StakeHolderType = RoleInTeamConstant.INVESTOR,
                    User = investor,
                    UserId = investor.Id,
                    ShareQuantity = investmentContractCreateDTO.InvestorInfo.ShareQuantity,
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
                        CreatedBy = userInProject.User.FullName,
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
                await _signNowService.AuthenticateAsync();
                contract.SignNowDocumentId = await _signNowService.UploadInvestmentContractToSignNowAsync(contract,investor,leader,userInProject.Project,shareEquity,disbursementList,investmentContractCreateDTO.InvestorInfo.BuyPrice);
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
            await _userService.CheckIfUserInProject(userId, projectId);
            var contracts = await _contractRepository.GetContractsByUserIdInAProject(userId, projectId, pageIndex, pageSize);
            return contracts;
        }
        

        public async Task<Contract> SendSigningInvitationForContract(string userId, string contractId)
        {
            var userInChosenContract = await _userService.CheckIfUserBelongToContract(userId, contractId);
            try
            {
                await _signNowService.AuthenticateAsync();
                var userPartys = new List<User>();
                foreach (var userInContract in userInChosenContract.Contract.UserContracts)
                {
                    User userParty = await _userManager.FindByIdAsync(userInContract.UserId);
                    userPartys.Add(userParty);
                }
                var userEmails = new List<string>();
                foreach (var userParty in userPartys)
                {
                    userEmails.Add(userParty.Email);
                }
                userInChosenContract.Contract.LastUpdatedBy = userInChosenContract.User.FullName;
                userInChosenContract.Contract.LastUpdatedTime = DateTimeOffset.UtcNow;
                userInChosenContract.Contract.ContractStatus = ContractStatusEnum.SENT;
                var inviteResponse = await _signNowService.CreateFreeFormInvite(userInChosenContract.Contract.SignNowDocumentId, userEmails);
                var webhookCompleteSign = new SignNowWebhookCreateDTO
                {
                    Action = SignNowServiceConstant.CallBackAction,
                    CallBackUrl = $"{_apiDomain}/api/contracts/valid-contract/{contractId}",
                    EntityId = userInChosenContract.Contract.SignNowDocumentId,
                    Event = SignNowServiceConstant.DocumentCompleteEvent
                };
                await _signNowService.RegisterWebhookAsync(webhookCompleteSign);
                var webhookUpdate = new SignNowWebhookCreateDTO
                {
                    Action = SignNowServiceConstant.CallBackAction,
                    CallBackUrl = $"{_apiDomain}/api/contracts/update-user-sign/{contractId}",
                    EntityId = userInChosenContract.Contract.SignNowDocumentId,
                    Event = SignNowServiceConstant.DocumentUpdateEvent
                };
                var webHookCreateList = new List<SignNowWebhookCreateDTO> { webhookCompleteSign, webhookUpdate };
                await _signNowService.RegisterManyWebhookAsync(webHookCreateList);
                _contractRepository.Update(userInChosenContract.Contract);
                await _unitOfWork.SaveChangesAsync();
                return userInChosenContract.Contract;
            }

            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while upload the contract file: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public async Task UpdateSignedStatusForUserInContract(string contractId) 
        {
            var contract = await _contractRepository.GetContractById(contractId);
            if (contract == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundContractError);
            }

            // Get the SignNow document invite data
            var contractInvite = await _signNowService.GetDocumentFreeFormInvite(contract.SignNowDocumentId);

            // Check if the contractInvite has data
            if (contractInvite?.Data == null || !contractInvite.Data.Any())
            {
                throw new InvalidOperationException("No signing data available from SignNow.");
            }

            // Loop through the signing data to find fulfilled users
            foreach (var signedData in contractInvite.Data)
            {
                if (signedData.Status.Equals(SignNowServiceConstant.FulfilledStatus))
                {
                    // Find the corresponding UserContract entry for the signed user
                    var existingUserContract = contract.UserContracts
                        .FirstOrDefault(x => x.User.Email.Equals(signedData.Email, StringComparison.OrdinalIgnoreCase));

                    if (existingUserContract == null)
                    {
                        throw new InvalidOperationException($"UserContract not found for the user with email: {signedData.Email}");
                    }

                    if (!existingUserContract.SignedDate.HasValue)
                    {
                        // Update the signed date
                        existingUserContract.SignedDate = DateOnly.FromDateTime(DateTime.UtcNow);
                    }
                }
            }
            await _unitOfWork.SaveChangesAsync();
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
                chosenContract.ContractStatus = ContractStatusEnum.COMPLETED;
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
        public async Task<DocumentDownLoadResponseDTO> DownLoadFileContract(string userId, string contractId)
        {
            var userInContract = await _userService.CheckIfUserBelongToContract(userId, contractId);
            await _signNowService.AuthenticateAsync();
            var documentDownloadinfo = await _signNowService.DownLoadDocument(userInContract.Contract.SignNowDocumentId);
            return documentDownloadinfo;
        }

        public async Task<IEnumerable<Contract>> SearchContractWithFilters(string userId, string projectId, ContractSearchDTO search, int pageIndex, int pageSize)
        {
            var userProject = await _userService.CheckIfUserInProject(userId, projectId);
            var searchResult = _contractRepository.QueryHelper().Include(c => c.UserContracts).Filter(x=>x.ProjectId.Equals(projectId) && x.UserContracts.Any(us => us.UserId.Equals(userId)));
            if (!string.IsNullOrEmpty(search.ContractName))
            {
                searchResult = searchResult.Filter(c => c.ContractName.Contains(search.ContractName.ToLower()));
            }

            if (search.ContractTypeEnum != null)
            {
                searchResult = searchResult.Filter(c => c.ContractType.Equals(search.ContractTypeEnum));
            }

            if (search.Parties?.Any() == true)
            {
                foreach (var party in search.Parties)
                {
                    searchResult = searchResult.Filter(c => c.UserContracts.Any(uc => uc.UserId.Equals(party)));
                }
            }

            if (search.LastUpdatedStartDate != default || search.LastUpdatedEndDate != default)
            {
                if (search.LastUpdatedStartDate != default)
                {
                    searchResult = searchResult.Filter(c => c.LastUpdatedTime >= search.LastUpdatedStartDate);
                }

                if (search.LastUpdatedEndDate != default)
                {
                    searchResult = searchResult.Filter(c => c.LastUpdatedTime <= search.LastUpdatedEndDate);
                }
            }
            
            if (search.ContractStatusEnum != null)
            {
                searchResult = searchResult.Filter(c => c.ContractStatus.Equals(search.ContractStatusEnum));
            }

            var searchResultList = await searchResult.GetPagingAsync(pageIndex, pageSize);

            // Mapping contracts and their users to the DTOs
            foreach (var contract in searchResultList)
            {
                foreach (var userConstract in contract.UserContracts) {
                    var userParty = await _userManager.FindByIdAsync(userConstract.UserId);
                    userConstract.User = userParty;
                }
            };

            return searchResultList;
        }
    }
}
