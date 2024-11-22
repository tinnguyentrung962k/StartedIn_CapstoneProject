using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using StartedIn.CrossCutting.Enum;
using CrossCutting.Exceptions;
using Microsoft.EntityFrameworkCore;
using StartedIn.Repository.Repositories.Extensions;
using StartedIn.Repository.Repositories;
using StartedIn.CrossCutting.DTOs.RequestDTO.Contract;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Contract;
using StartedIn.CrossCutting.DTOs.RequestDTO.SignNow.SignNowWebhookRequestDTO;
using StartedIn.CrossCutting.DTOs.BaseDTO;
using DocumentFormat.OpenXml.Bibliography;
using StartedIn.CrossCutting.DTOs.ResponseDTO;

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
        private readonly IDealOfferRepository _dealOfferRepository;
        private readonly IFinanceRepository _financeRepository;
        private readonly IInvestmentCallService _investmentCallService;
        private readonly IInvestmentCallRepository _investmentCallRepository;

        public ContractService(IContractRepository contractRepository,
            IUnitOfWork unitOfWork,
            ILogger<Contract> logger,
            UserManager<User> userManager,
            ISignNowService signNowService,
            IEmailService emailService,
            IProjectRepository projectRepository,
            IShareEquityRepository shareEquityRepository,
            IDisbursementRepository disbursementRepository,
            IAzureBlobService azureBlobService, 
            IDocumentFormatService documentFormatService,
            IConfiguration configuration,
            IUserService userService,
            IDealOfferRepository dealOfferRepository,
            IFinanceRepository financeRepository,
            IInvestmentCallService investmentCallService,
            IInvestmentCallRepository investmentCallRepository
            )
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
            _dealOfferRepository = dealOfferRepository;
            _financeRepository = financeRepository;
            _apiDomain = _configuration.GetValue<string>("API_DOMAIN") ?? _configuration["Local_domain"];
            _investmentCallService = investmentCallService;
            _investmentCallRepository = investmentCallRepository;

        }

        public async Task<Contract> CreateInvestmentContract(string userId, string projectId, InvestmentContractCreateDTO investmentContractCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var existedContract = await _contractRepository.QueryHelper().Filter(x => x.ContractIdNumber.Equals(investmentContractCreateDTO.Contract.ContractIdNumber) && x.ProjectId.Equals(projectId)).GetOneAsync();
            if (existedContract != null)
            {
                throw new ExistedRecordException(MessageConstant.ContractNumberExistedError);
            }
            var project = await _projectRepository.GetProjectById(projectId);
            if (investmentContractCreateDTO.InvestorInfo.Percentage > project.RemainingPercentOfShares)
            {
                throw new InvalidOperationException(MessageConstant.DealPercentageGreaterThanRemainingPercentage);
            }
            if (investmentContractCreateDTO.Disbursements == null)
            {
                throw new InvalidDataException(MessageConstant.DisbursementListEmptyInContract);
            }
            decimal totalDisbursementAmount = investmentContractCreateDTO.Disbursements.Sum(d => d.Amount);
            if (totalDisbursementAmount > investmentContractCreateDTO.InvestorInfo.BuyPrice)
            {
                throw new InvalidOperationException(MessageConstant.DisbursementGreaterThanBuyPriceError);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var investor = await _userManager.FindByIdAsync(investmentContractCreateDTO.InvestorInfo.UserId);
                if (investor == null)
                {
                    throw new NotFoundException(MessageConstant.NotFoundInvestorError);
                }
                Contract contract = new Contract
                {
                    ContractName = investmentContractCreateDTO.Contract.ContractName,
                    ContractPolicy = investmentContractCreateDTO.Contract.ContractPolicy,
                    ContractType = ContractTypeEnum.INVESTMENT,
                    CreatedBy = userInProject.User.FullName,
                    ProjectId = projectId,
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
                    StakeHolderType = RoleInTeam.Investor,
                    User = investor,
                    UserId = investor.Id,
                    SharePrice = investmentContractCreateDTO.InvestorInfo.BuyPrice
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
                        DisbursementStatus = DisbursementStatusEnum.PENDING,
                        StartDate = disbursementTime.StartDate,
                        EndDate = disbursementTime.EndDate,
                        Title = disbursementTime.Title,
                        Investor = investor,
                        InvestorId = investor.Id,
                        IsValidWithContract = false
                    };
                    disbursementList.Add(disbursement);
                    var disbursementEntity = _disbursementRepository.Add(disbursement);
                }
                await _signNowService.AuthenticateAsync();
                contract.SignNowDocumentId = await _signNowService.UploadInvestmentContractToSignNowAsync(contract,investor,leader,userInProject.Project,shareEquity,disbursementList);
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
        public async Task<Contract> CreateStartupShareAllMemberContract(string userId, string projectId,GroupContractCreateDTO groupContractCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var existedContractWithIdNumber = await _contractRepository.QueryHelper().Filter(x => x.ContractIdNumber.Equals(groupContractCreateDTO.Contract.ContractIdNumber) && x.ProjectId.Equals(projectId)).GetOneAsync();
            if (existedContractWithIdNumber != null)
            {
                throw new ExistedRecordException(MessageConstant.ContractNumberExistedError);
            }
            if (groupContractCreateDTO.ShareEquitiesOfMembers == null)
            {
                throw new InvalidDataException(MessageConstant.ShareDistributionListEmptyInContract);
            }
            var project = await _projectRepository.GetProjectById(projectId);
            var totalShareOfDistribution = groupContractCreateDTO.ShareEquitiesOfMembers.Sum(d => d.Percentage);
            if (totalShareOfDistribution > project.RemainingPercentOfShares)
            {
                throw new InvalidOperationException(MessageConstant.TotalDistributePercentageGreaterThanRemainingPercentage);
            }
            var existedDistributeContractInProject = await _contractRepository.QueryHelper().Filter(x => x.ProjectId.Equals(projectId) && x.ContractType.Equals(ContractTypeEnum.INTERNAL) && x.ContractStatus == ContractStatusEnum.COMPLETED).GetOneAsync();
            if (existedDistributeContractInProject != null)
            {
                throw new ExistedRecordException(MessageConstant.ValidShareDistributionContractExisted);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                Contract contract = new Contract
                {
                    ContractName = groupContractCreateDTO.Contract.ContractName,
                    ContractPolicy = groupContractCreateDTO.Contract.ContractPolicy,
                    ContractType = ContractTypeEnum.INTERNAL,
                    CreatedBy = userInProject.User.FullName,
                    ProjectId = projectId,
                    ContractStatus = ContractStatusEnum.DRAFT,
                    ContractIdNumber = groupContractCreateDTO.Contract.ContractIdNumber
                };

                List<UserContract> usersInContract = new List<UserContract>();
                List<ShareEquity> shareEquitiesOfMembers = new List<ShareEquity>();
                foreach (var shareEquityOfAMember in groupContractCreateDTO.ShareEquitiesOfMembers)
                {
                    var userMemberInContract = await _userManager.FindByIdAsync(shareEquityOfAMember.UserId);
                    var projectRoleOfMember = await _projectRepository.GetUserRoleInProject(shareEquityOfAMember.UserId, projectId);
                    UserContract userInContract = new UserContract
                    {
                        UserId = userMemberInContract.Id,
                        ContractId = contract.Id,
                        Contract = contract,
                        User = userMemberInContract,
                    };
                    usersInContract.Add(userInContract);
                    ShareEquity shareEquity = new ShareEquity
                    {
                        ContractId = contract.Id,
                        Contract = contract,
                        CreatedBy = userInProject.User.FullName,
                        Percentage = shareEquityOfAMember.Percentage,
                        StakeHolderType = projectRoleOfMember,
                        User = userMemberInContract,
                        UserId = shareEquityOfAMember.UserId
                    };
                    shareEquitiesOfMembers.Add(shareEquity);
                }
                var leader = userInProject.User;
                contract.UserContracts = usersInContract;
                var contractEntity = _contractRepository.Add(contract);
                await _shareEquityRepository.AddRangeAsync(shareEquitiesOfMembers);
                await _signNowService.AuthenticateAsync();
                contract.SignNowDocumentId = await _signNowService.UploadStartUpShareDistributionContractToSignNowAsync(contract, leader, userInProject.Project, shareEquitiesOfMembers, usersInContract);
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
        public async Task<Contract> CreateInvestmentContractFromDeal(
            string userId,
            string projectId,
            InvestmentContractFromDealCreateDTO investmentContractFromDealCreateDTO
            )
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var existedContract = await _contractRepository.QueryHelper().Filter(x => x.ContractIdNumber.Equals(investmentContractFromDealCreateDTO.Contract.ContractIdNumber) && x.ProjectId.Equals(projectId)).GetOneAsync();
            if (existedContract != null)
            {
                throw new ExistedRecordException(MessageConstant.ContractNumberExistedError);
            }
            var project = await _projectRepository.GetProjectById(projectId);
            var chosenDeal = await _dealOfferRepository.QueryHelper()
                .Include(x => x.Investor)
                .Include(x => x.Project)
                .Filter(x => x.Id.Equals(investmentContractFromDealCreateDTO.DealId))
                .GetOneAsync();
            if (chosenDeal == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundDealError);
            }
            if (chosenDeal.Project.Id != projectId)
            {
                throw new UnmatchedException(MessageConstant.DealNotBelongToProjectError);
            }
            if (chosenDeal.EquityShareOffer > project.RemainingPercentOfShares)
            {
                throw new InvalidOperationException(MessageConstant.DealPercentageGreaterThanRemainingPercentage);
            }
            if (chosenDeal.DealStatus != DealStatusEnum.Accepted)
            {
                throw new ContractConfirmException(MessageConstant.DealNotAccepted);
            }
            if (investmentContractFromDealCreateDTO.Disbursements == null)
            {
                throw new InvalidDataException(MessageConstant.DisbursementListEmptyInContract);
            }
            decimal totalDisbursementAmount = investmentContractFromDealCreateDTO.Disbursements.Sum(d => d.Amount);
            if (totalDisbursementAmount > chosenDeal.Amount)
            {
                throw new InvalidOperationException(MessageConstant.DisbursementGreaterThanBuyPriceError);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var investor = await _userManager.FindByIdAsync(chosenDeal.InvestorId);
                if (investor == null)
                {
                    throw new NotFoundException(MessageConstant.NotFoundInvestorError);
                }
                Contract contract = new Contract
                {
                    ContractName = investmentContractFromDealCreateDTO.Contract.ContractName,
                    ContractPolicy = investmentContractFromDealCreateDTO.Contract.ContractPolicy,
                    ContractType = ContractTypeEnum.INVESTMENT,
                    CreatedBy = userInProject.User.FullName,
                    ProjectId = projectId,
                    ContractStatus = ContractStatusEnum.DRAFT,
                    ContractIdNumber = investmentContractFromDealCreateDTO.Contract.ContractIdNumber,
                    DealOffer = chosenDeal,
                    DealOfferId = chosenDeal.Id
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
                    Percentage = chosenDeal.EquityShareOffer,
                    StakeHolderType = RoleInTeam.Investor,
                    User = investor,
                    UserId = investor.Id,
                    SharePrice = chosenDeal.Amount
                    
                };
                var contractEntity = _contractRepository.Add(contract);
                var shareEquityEntity = _shareEquityRepository.Add(shareEquity);
                var disbursementList = new List<Disbursement>();
                foreach (var disbursementTime in investmentContractFromDealCreateDTO.Disbursements)
                {
                    Disbursement disbursement = new Disbursement
                    {
                        Amount = disbursementTime.Amount,
                        Condition = disbursementTime.Condition,
                        Contract = contract,
                        ContractId = contract.Id,
                        CreatedBy = userInProject.User.FullName,
                        DisbursementStatus = DisbursementStatusEnum.PENDING,
                        StartDate = disbursementTime.StartDate,
                        EndDate = disbursementTime.EndDate,
                        Title = disbursementTime.Title,
                        Investor = investor,
                        InvestorId = investor.Id,
                        IsValidWithContract = false
                    };
                    disbursementList.Add(disbursement);
                    var disbursementEntity = _disbursementRepository.Add(disbursement);
                }
                await _signNowService.AuthenticateAsync();
                contract.SignNowDocumentId = await _signNowService
                    .UploadInvestmentContractToSignNowAsync(contract, investor, leader, userInProject.Project, shareEquity, disbursementList);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return contract;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while creating the contract: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<Contract> GetContractByContractId(string userId, string id, string projectId)
        {
            var project = await _projectRepository.GetProjectById(projectId);
            if (project is null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }
            var chosenContract = await _contractRepository.GetContractById(id);
            if (chosenContract == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundContractError);
            }
            if (projectId != chosenContract.ProjectId)
            {
                throw new UnmatchedException(MessageConstant.ContractNotBelongToProjectError);
            }
            var userProject = await _userService.CheckIfUserInProject(userId, projectId);
            return chosenContract;
        }
        

        public async Task<Contract> SendSigningInvitationForContract(string projectId, string userId, string contractId)
        {
            var project = await _projectRepository.GetProjectById(projectId);
            if (project is null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }
            var contract = await _contractRepository.GetContractById(contractId);
            if (contract is null)
            {
                throw new NotFoundException(MessageConstant.NotFoundContractError);
            }
            if (projectId != contract.ProjectId) 
            {
                throw new UnmatchedException(MessageConstant.ContractNotBelongToProjectError);    
            }
            if (contract.ContractStatus != ContractStatusEnum.DRAFT) {
                throw new StatusException(MessageConstant.CannotInviteToSign);
            }
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
                userInChosenContract.Contract.SignDeadline = DateTimeOffset.UtcNow.AddDays(7);
                var inviteResponse = await _signNowService.CreateFreeFormInvite(userInChosenContract.Contract.SignNowDocumentId, userEmails);
                var webHookCreateList = new List<SignNowWebhookCreateDTO>();
                if (contract.ContractType == ContractTypeEnum.INVESTMENT)
                {
                    var investor = await GetAUserFromContractWithUserRole(contractId);
                    var webhookAddInvestorToProject = new SignNowWebhookCreateDTO
                    {
                        Action = SignNowServiceConstant.CallBackAction,
                        CallBackUrl = $"{_apiDomain}/api/projects/{projectId}/add-user/{investor.Id}/{RoleInTeam.Investor}",
                        EntityId = userInChosenContract.Contract.SignNowDocumentId,
                        Event = SignNowServiceConstant.DocumentCompleteEvent,
                    };
                    webHookCreateList.Add(webhookAddInvestorToProject);
                }
                var webhookCompleteSign = new SignNowWebhookCreateDTO
                {
                    Action = SignNowServiceConstant.CallBackAction,
                    CallBackUrl = $"{_apiDomain}/api/projects/{projectId}/contracts/{contractId}/validate",
                    EntityId = userInChosenContract.Contract.SignNowDocumentId,
                    Event = SignNowServiceConstant.DocumentCompleteEvent
                };
                var webhookUpdate = new SignNowWebhookCreateDTO
                {
                    Action = SignNowServiceConstant.CallBackAction,
                    CallBackUrl = $"{_apiDomain}/api/projects/{projectId}/contracts/{contractId}/confirm-sign",
                    EntityId = userInChosenContract.Contract.SignNowDocumentId,
                    Event = SignNowServiceConstant.DocumentUpdateEvent
                };
                webHookCreateList.AddRange(new List<SignNowWebhookCreateDTO>{webhookCompleteSign, webhookUpdate});
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
        public async Task<User> GetAUserFromContractWithUserRole(string contractId)
        {
            var contract = await _contractRepository.GetContractById(contractId);
            if (contract == null) {
                throw new NotFoundException(MessageConstant.NotFoundContractError);
            }
            var user = await _userManager.GetAUserInAContractWithSystemRole(contractId, RoleConstants.INVESTOR);
            if (user == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundUserError);
            }
            return user;
        }
        public async Task UpdateSignedStatusForUserInContract(string contractId, string projectId) 
        {
            var project = await _projectRepository.GetProjectById(projectId);
            if (project == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }
            var chosenContract = await _contractRepository.GetContractById(contractId);
            if (chosenContract == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundContractError);
            }
            if (projectId != chosenContract.ProjectId)
            {
                throw new UnmatchedException(MessageConstant.ContractNotBelongToProjectError);
            }

            // Get the SignNow document invite data
            var contractInvite = await _signNowService.GetDocumentFreeFormInvite(chosenContract.SignNowDocumentId);

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
                    var existingUserContract = chosenContract.UserContracts
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
        public async Task<Contract> ValidateContractOnSignedAsync(string id,string projectId)
        {
            var project = await _projectRepository.GetProjectById(projectId);
            if (project == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }
            var chosenContract = await _contractRepository.GetContractById(id);
            if (chosenContract == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundContractError);
            }
            if (projectId != chosenContract.ProjectId)
            {
                throw new UnmatchedException(MessageConstant.ContractNotBelongToProjectError);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                chosenContract.ValidDate = DateOnly.FromDateTime(DateTime.Now);
                chosenContract.ContractStatus = ContractStatusEnum.COMPLETED;
                foreach (var equityShare in chosenContract.ShareEquities)
                {
                    equityShare.DateAssigned = DateOnly.FromDateTime(DateTime.Now);
                    _shareEquityRepository.Update(equityShare);
                }
                var totalEquitiesSharePercentageInContract = chosenContract.ShareEquities.Sum(e => e.Percentage);
                project.RemainingPercentOfShares = (decimal)(project.RemainingPercentOfShares - totalEquitiesSharePercentageInContract);
                foreach (var disbursement in chosenContract.Disbursements)
                {
                    disbursement.IsValidWithContract = true;
                    _disbursementRepository.Update(disbursement);
                }
                if (chosenContract.DealOffer.InvestmentCallId != null)
                {
                    var investmentCall = await _investmentCallService.GetInvestmentCallById(projectId, chosenContract.DealOffer.InvestmentCallId);
                    investmentCall.AmountRaised += chosenContract.DealOffer.Amount;
                    investmentCall.RemainAvailableEquityShare -= chosenContract.DealOffer.EquityShareOffer;
                    investmentCall.TotalInvestor++;
                    if (investmentCall.RemainAvailableEquityShare == 0)
                    {
                        investmentCall.Status = InvestmentCallStatus.Closed;
                    }
                    _investmentCallRepository.Update(investmentCall);
                }
                var totalDisbursement = chosenContract.Disbursements.Sum(e => e.Amount);
                var projectFinance = await _financeRepository.QueryHelper()
                    .Filter(x => x.ProjectId.Equals(projectId))
                    .GetOneAsync();
                projectFinance.RemainingDisbursement += totalDisbursement;
                _contractRepository.Update(chosenContract);
                _financeRepository.Update(projectFinance);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return chosenContract;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while update the contract: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public async Task<DocumentDownLoadResponseDTO> DownLoadFileContract(string userId, string projectId, string contractId)
        {
            var contract = await _contractRepository.GetContractById(contractId);
            if (contract == null) {
                throw new NotFoundException(MessageConstant.NotFoundContractError);
            }
            if (projectId != contract.ProjectId)
            {
                throw new UnmatchedException(MessageConstant.ContractNotBelongToProjectError);
            }
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var userInContract = await _userService.CheckIfUserBelongToContract(userId, contractId);
            await _signNowService.AuthenticateAsync();
            var documentDownloadinfo = await _signNowService.DownLoadDocument(userInContract.Contract.SignNowDocumentId);
            return documentDownloadinfo;
        }

        public async Task<PaginationDTO<ContractSearchResponseDTO>> SearchContractWithFilters(string userId, string projectId, ContractSearchDTO search, int page, int size)
        {
            var userProject = await _userService.CheckIfUserInProject(userId, projectId);
            var searchResult = _contractRepository.GetContractListQuery(userId, projectId);
            // Filter by Contract Name
            if (!string.IsNullOrWhiteSpace(search.ContractName))
            {
                string contractNameLower = search.ContractName.ToLower();
                searchResult = searchResult.Where(c => c.ContractName.ToLower().Contains(contractNameLower));
            }

            // Filter by Contract Type Enum
            if (search.ContractTypeEnum.HasValue)
            {
                searchResult = searchResult.Where(c => c.ContractType == search.ContractTypeEnum.Value);
            }

            // Filter by Parties (User IDs)
            if (search.Parties?.Any() == true)
            {
                var partyIds = search.Parties;

                // Filter contracts that contain all the specified parties
                searchResult = searchResult.Where(c =>
                    partyIds.All(partyId => c.UserContracts.Any(uc => uc.UserId == partyId)));
            }

            // Filter by Last Updated Date Range
            if (search.LastUpdatedStartDate.HasValue)
            {
                searchResult = searchResult.Where(c => c.LastUpdatedTime >= search.LastUpdatedStartDate.Value);
            }

            if (search.LastUpdatedEndDate.HasValue)
            {
                searchResult = searchResult.Where(c => c.LastUpdatedTime <= search.LastUpdatedEndDate.Value);
            }

            // Filter by Contract Status Enum
            if (search.ContractStatusEnum.HasValue)
            {
                searchResult = searchResult.Where(c => c.ContractStatus == search.ContractStatusEnum.Value);
            }

            int totalCount = await searchResult.CountAsync();

            var pagedResult = await searchResult
                .Skip((page - 1) * size)
                .Take(size)
                .Include(c => c.UserContracts)
                .ThenInclude(uc => uc.User)
                .ToListAsync();

            var contractSearchResponseDTOs = pagedResult.Select(contract => new ContractSearchResponseDTO
            {
                Id = contract.Id,
                ContractName = contract.ContractName,
                ContractStatus = contract.ContractStatus,
                ContractType = contract.ContractType,
                LastUpdatedTime = contract.LastUpdatedTime,
                Parties = contract.UserContracts.Select(userContract => new UserInContractResponseDTO
                {
                    Id = userContract.User.Id,
                    Email = userContract.User.Email,
                    FullName = userContract.User.FullName,
                    PhoneNumber = userContract.User.PhoneNumber
                }).ToList()
            }).ToList();

            // Construct pagination DTO
            var response = new PaginationDTO<ContractSearchResponseDTO>
            {
                Data = contractSearchResponseDTOs,
                Total = totalCount,
                Size = size,
                Page = page
            };

            return response;
        }

        public async Task<Contract> UpdateInvestmentContract(string userId, string projectId ,string contractId, InvestmentContractUpdateDTO investmentContractUpdateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var leader = await _userManager.FindByIdAsync(userId);
            var contract = await _contractRepository.GetContractById(contractId);
            if (contract == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundContractError);
            }
            if (contract.ContractStatus != ContractStatusEnum.DRAFT)
            {
                throw new UpdateException(MessageConstant.CannotEditContractError);
            }
            var project = await _projectRepository.GetProjectById(projectId);
            if (investmentContractUpdateDTO.InvestorInfo.Percentage > project.RemainingPercentOfShares)
            {
                throw new InvalidOperationException(MessageConstant.DealPercentageGreaterThanRemainingPercentage);
            }
            decimal totalDisbursementAmount = investmentContractUpdateDTO.Disbursements.Sum(d => d.Amount);
            if (totalDisbursementAmount > investmentContractUpdateDTO.InvestorInfo.BuyPrice)
            {
                throw new InvalidOperationException(MessageConstant.DisbursementGreaterThanBuyPriceError);
            }

            try
            {
                _unitOfWork.BeginTransaction();

                // Update contract details
                contract.ContractName = investmentContractUpdateDTO.Contract.ContractName;
                contract.ContractPolicy = investmentContractUpdateDTO.Contract.ContractPolicy;
                contract.ContractIdNumber = investmentContractUpdateDTO.Contract.ContractIdNumber;

                // Update or add ShareEquity details
                var investor = await _userManager.FindByIdAsync(investmentContractUpdateDTO.InvestorInfo.UserId);
                if (investor == null)
                {
                    throw new NotFoundException(MessageConstant.NotFoundInvestorError);
                }

                ShareEquity shareEquity;
                var existingShareEquity = contract.ShareEquities.FirstOrDefault(se => se.UserId == investor.Id);

                if (existingShareEquity != null)
                {
                    // Update existing ShareEquity
                    existingShareEquity.Percentage = investmentContractUpdateDTO.InvestorInfo.Percentage;
                    existingShareEquity.SharePrice = investmentContractUpdateDTO.InvestorInfo.BuyPrice;
                    existingShareEquity.LastUpdatedBy = leader.FullName;
                    _shareEquityRepository.Update(existingShareEquity);
                    shareEquity = existingShareEquity;
                }
                else
                {
                    // Add a new ShareEquity if none exists for this investor
                    shareEquity = new ShareEquity
                    {
                        ContractId = contract.Id,
                        UserId = investor.Id,
                        Percentage = investmentContractUpdateDTO.InvestorInfo.Percentage,
                        StakeHolderType = RoleInTeam.Investor,
                        CreatedBy = leader.FullName,
                    };
                    _shareEquityRepository.Add(shareEquity);
                }
                var existingDisbursements = contract.Disbursements;
                foreach (var existingDisbursement in existingDisbursements)
                {
                   await _disbursementRepository.DeleteAsync(existingDisbursement);
                }
                var disbursementList = new List<Disbursement>();

                foreach (var updatedDisbursement in investmentContractUpdateDTO.Disbursements)
                {
                    var newDisbursement = new Disbursement
                    {
                       ContractId = contract.Id,
                       Amount = updatedDisbursement.Amount,
                       Condition = updatedDisbursement.Condition,
                       StartDate = updatedDisbursement.StartDate,
                       EndDate = updatedDisbursement.EndDate,
                       Title = updatedDisbursement.Title,
                       CreatedBy = userId,
                       DisbursementStatus = DisbursementStatusEnum.PENDING,
                       InvestorId = investor.Id,
                       IsValidWithContract = false
                    };
                    _disbursementRepository.Add(newDisbursement);
                    disbursementList.Add(newDisbursement);
                }


                // Upload updated contract to SignNow
                await _signNowService.AuthenticateAsync();
                contract.SignNowDocumentId = await _signNowService.UploadInvestmentContractToSignNowAsync(
                    contract,
                    investor,
                    leader,
                    userInProject.Project,
                    shareEquity,
                    disbursementList
                );

                var contractEntity = _contractRepository.Update(contract);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return contractEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the contract.");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public async Task<Contract> UpdateStartupShareAllMemberContract(string userId, string projectId, string contractId, GroupContractUpdateDTO groupContractUpdateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var contract = await _contractRepository.GetContractById(contractId);
            if (contract == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundContractError);
            }
            if (contract.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.CharterNotBelongToProjectError);
            }
            var currentUserInContract = await _userService.CheckIfUserBelongToContract(userId, contractId);
            if (contract.ContractStatus != ContractStatusEnum.DRAFT)
            {
                throw new UpdateException(MessageConstant.CannotEditContractError);
            }
            var existedContractWithIdNumber = await _contractRepository.QueryHelper().Filter(x => x.ContractIdNumber.Equals(groupContractUpdateDTO.Contract.ContractIdNumber) && x.ProjectId.Equals(projectId) && !x.Id.Equals(contractId)).GetOneAsync();
            if (existedContractWithIdNumber != null)
            {
                throw new ExistedRecordException(MessageConstant.ContractNumberExistedError);
            }
            var project = await _projectRepository.GetProjectById(projectId);
            var totalShareOfDistribution = groupContractUpdateDTO.ShareEquitiesOfMembers.Sum(d => d.Percentage);
            if (totalShareOfDistribution > project.RemainingPercentOfShares)
            {
                throw new InvalidOperationException(MessageConstant.TotalDistributePercentageGreaterThanRemainingPercentage);
            }
            try
            {
                _unitOfWork.BeginTransaction();

                // Update contract details
                contract.ContractName = groupContractUpdateDTO.Contract.ContractName;
                contract.ContractPolicy = groupContractUpdateDTO.Contract.ContractPolicy;
                contract.ContractIdNumber = groupContractUpdateDTO.Contract.ContractIdNumber;

                var existingShareEquitiesOfMembers = contract.ShareEquities;
                foreach (var existingShareEquity in existingShareEquitiesOfMembers)
                {
                    await _shareEquityRepository.DeleteAsync(existingShareEquity);
                }

                List<UserContract> usersInContract = new List<UserContract>();
                List<ShareEquity> shareEquitiesOfMembers = new List<ShareEquity>();
                foreach (var updatedShareEquity in groupContractUpdateDTO.ShareEquitiesOfMembers)
                {
                    var userMemberInContract = await _userManager.FindByIdAsync(updatedShareEquity.UserId);
                    var projectRoleOfMember = await _projectRepository.GetUserRoleInProject(updatedShareEquity.UserId, projectId);
                    UserContract userInContract = new UserContract
                    {
                        UserId = userMemberInContract.Id,
                        ContractId = contract.Id,
                        Contract = contract,
                        User = userMemberInContract,
                    };
                    usersInContract.Add(userInContract);
                    ShareEquity shareEquity = new ShareEquity
                    {
                        ContractId = contract.Id,
                        Contract = contract,
                        CreatedBy = userInProject.User.FullName,
                        Percentage = updatedShareEquity.Percentage,
                        StakeHolderType = projectRoleOfMember,
                        User = userMemberInContract,
                        UserId = updatedShareEquity.UserId
                    };
                    shareEquitiesOfMembers.Add(shareEquity);
                }
                var leader = userInProject.User;
                contract.UserContracts = usersInContract;
                var contractEntity = _contractRepository.Update(contract);
                await _shareEquityRepository.AddRangeAsync(shareEquitiesOfMembers);
                await _signNowService.AuthenticateAsync();
                contract.SignNowDocumentId = await _signNowService.UploadStartUpShareDistributionContractToSignNowAsync(contract, leader, userInProject.Project, shareEquitiesOfMembers, usersInContract);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return contractEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the contract.");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public async Task<Contract> CancelContract(string userId, string projectId, string contractId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var loginUser = await _userManager.FindByIdAsync(userId);
            var chosenContract = await _contractRepository.GetContractById(contractId);
            if (chosenContract == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundContractError);
            }
            if (chosenContract.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.ContractNotBelongToProjectError);
            }
            var userInContract = await _userService.CheckIfUserBelongToContract(userId, contractId);
            if (chosenContract.ContractStatus == ContractStatusEnum.COMPLETED || chosenContract.ContractStatus == ContractStatusEnum.EXPIRED)
            {
                throw new UpdateException(MessageConstant.CannotCancelContractError);
            }
            try {
                _unitOfWork.BeginTransaction();
                chosenContract.ContractStatus = ContractStatusEnum.CANCELLED;
                _contractRepository.Update(chosenContract);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return chosenContract;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the contract.");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public async Task MarkExpiredContract(string userId, string projectId, string contractId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var contract = await _contractRepository.GetContractById(contractId);
            if (contract.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.ContractNotBelongToProjectError);
            }
            var userInContract = await _userService.CheckIfUserBelongToContract(userId, contractId);
            try
            {
                _unitOfWork.BeginTransaction();
                contract.ExpiredDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
                contract.ContractStatus = ContractStatusEnum.EXPIRED;
                contract.LastUpdatedBy = userInContract.User.FullName;
                contract.LastUpdatedTime = DateTimeOffset.UtcNow;
                if (contract.ContractType == ContractTypeEnum.INTERNAL)
                {
                    decimal expiredContractShare = (decimal)contract.ShareEquities.Sum(x => x.Percentage);
                    userInProject.Project.RemainingPercentOfShares += expiredContractShare;
                    _logger.LogInformation($"Hợp đồng nội bộ hết hạn. Trả lại {expiredContractShare}% cổ phần cho dự án {userInProject.Project.ProjectName}. Tổng cổ phần còn lại: {userInProject.Project.RemainingPercentOfShares}%.");
                    _projectRepository.Update(userInProject.Project);
                }
                else if (contract.ContractType == ContractTypeEnum.INVESTMENT)
                {
                    // Xử lý khi hợp đồng đầu tư hết hạn
                    decimal investmentShare = (decimal)contract.ShareEquities.Sum(x => x.Percentage);

                    // Logic tuỳ chọn: Mua lại, hoàn trả, hoặc chuyển nhượng cổ phần
                    userInProject.Project.RemainingPercentOfShares += investmentShare;
                    _logger.LogInformation($"Hợp đồng đầu tư hết hạn. Trả lại {investmentShare}% cổ phần cho dự án {userInProject.Project.ProjectName}. Tổng cổ phần còn lại: {userInProject.Project.RemainingPercentOfShares}%.");
                    _projectRepository.Update(userInProject.Project);
                    // Logic tuỳ chọn: Xử lý tài chính hoặc thông báo
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xảy ra khi xử lý hết hạn hợp đồng: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }

        }

        public async Task CancelContractAfterDueDate()
        {
            var contracts = await _contractRepository.QueryHelper()
                .Filter(c => (c.ContractStatus == ContractStatusEnum.SENT ||
                             c.ContractStatus == ContractStatusEnum.DRAFT) &&
                             c.SignDeadline < DateTimeOffset.UtcNow.AddDays(1)).GetAllAsync();
            foreach (var contract in contracts)
            {
                contract.ContractStatus = ContractStatusEnum.CANCELLED; 
                _contractRepository.Update(contract);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
