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
using Microsoft.EntityFrameworkCore;
using StartedIn.Repository.Repositories.Extensions;
using StartedIn.CrossCutting.DTOs.RequestDTO.Contract;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Contract;
using StartedIn.CrossCutting.DTOs.RequestDTO.SignNow.SignNowWebhookRequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
using CrossCutting.Exceptions;

namespace StartedIn.Service.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Contract> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ISignNowService _signNowService;
        private readonly IProjectRepository _projectRepository;
        private readonly IShareEquityRepository _shareEquityRepository;
        private readonly IDisbursementRepository _disbursementRepository;
        private readonly IConfiguration _configuration;
        private readonly string _apiDomain;
        private readonly IUserService _userService;
        private readonly IDealOfferRepository _dealOfferRepository;
        private readonly IFinanceRepository _financeRepository;
        private readonly IInvestmentCallService _investmentCallService;
        private readonly IInvestmentCallRepository _investmentCallRepository;
        private readonly IAzureBlobService _azureBlobService;
        private readonly IDocumentFormatService _documentFormatService;
        private readonly IAppSettingManager _appSettingManager;
        private readonly ITerminationRequestRepository _terminationRequestRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IEmailService _emailService;
        private readonly ITransactionService _transactionService;

        public ContractService(IContractRepository contractRepository,
            IUnitOfWork unitOfWork,
            ILogger<Contract> logger,
            UserManager<User> userManager,
            ISignNowService signNowService,
            IProjectRepository projectRepository,
            IShareEquityRepository shareEquityRepository,
            IDisbursementRepository disbursementRepository,
            IConfiguration configuration,
            IUserService userService,
            IDealOfferRepository dealOfferRepository,
            IFinanceRepository financeRepository,
            IInvestmentCallService investmentCallService,
            IInvestmentCallRepository investmentCallRepository,
            IAzureBlobService azureBlobService,
            IDocumentFormatService documentFormatService,
            IAppSettingManager appSettingManager,
            ITerminationRequestRepository terminationRequestRepository,
            IAppointmentRepository appointmentRepository,
            IEmailService emailService,
            ITransactionService transactionService
        )
        {
            _contractRepository = contractRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _signNowService = signNowService;
            _projectRepository = projectRepository;
            _shareEquityRepository = shareEquityRepository;
            _disbursementRepository = disbursementRepository;
            _configuration = configuration;
            _userService = userService;
            _dealOfferRepository = dealOfferRepository;
            _financeRepository = financeRepository;
            _apiDomain = _configuration.GetValue<string>("API_DOMAIN") ?? _configuration["Local_domain"];
            _investmentCallService = investmentCallService;
            _investmentCallRepository = investmentCallRepository;
            _azureBlobService = azureBlobService;
            _documentFormatService = documentFormatService;
            _appSettingManager = appSettingManager;
            _terminationRequestRepository = terminationRequestRepository;
            _appointmentRepository = appointmentRepository;
            _emailService = emailService;
            _transactionService = transactionService;
        }

        public async Task<Contract> CreateInvestmentContract(string userId, string projectId, InvestmentContractCreateDTO investmentContractCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
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
            var investor = await _userManager.FindByIdAsync(investmentContractCreateDTO.InvestorInfo.UserId);
            if (investor == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundInvestorError);
            }
            if (string.IsNullOrWhiteSpace(investor.Address) || string.IsNullOrWhiteSpace(investor.PhoneNumber)
               || string.IsNullOrWhiteSpace(investor.IdCardNumber)) 
            {
                throw new InvalidInputException(MessageConstant.InvalidInformationOfUser + $" {investor.FullName}");
            }
            var leader = userInProject.User;
            if (string.IsNullOrWhiteSpace(leader.Address) || string.IsNullOrWhiteSpace(leader.PhoneNumber)
                || string.IsNullOrWhiteSpace(leader.IdCardNumber))
            {
                throw new InvalidInputException(MessageConstant.InvalidInformationOfUser + $" {leader.FullName}");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                string prefix = "HDDT";
                string currentDateTime = DateTimeOffset.UtcNow.AddHours(7).ToString("ddMMyyyyHHmm");
                string contractIdNumberGen = $"{prefix}-{currentDateTime}";
                Contract contract = new Contract
                {
                    ContractName = investmentContractCreateDTO.Contract.ContractName,
                    ContractPolicy = investmentContractCreateDTO.Contract.ContractPolicy,
                    ContractType = ContractTypeEnum.INVESTMENT,
                    CreatedBy = userInProject.User.FullName,
                    ProjectId = projectId,
                    ContractStatus = ContractStatusEnum.DRAFT,
                    ContractIdNumber = contractIdNumberGen,
                    ExpiredDate = investmentContractCreateDTO.Contract.ExpiredDate
                };
                List<UserContract> usersInContract = new List<UserContract>();
                List<User> chosenUsersList = new List<User> { investor, leader };
                foreach (var chosenUser in chosenUsersList)
                {
                    UserContract userContract = new UserContract
                    {
                        ContractId = contract.Id,
                        UserId = chosenUser.Id,
                        Role = chosenUser.Id == leader.Id ? RoleInContract.CREATOR : RoleInContract.SIGNER
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
                var signingMethod = await _appSettingManager.GetSettingAsync("SignatureType");
                if (signingMethod == SettingsValue.InternalApp)
                {
                    var modifiedMemoryStream = await _documentFormatService.ReplacePlaceHolderForInvestmentDocumentAsync(contract, investor, leader, project, shareEquity, disbursementList);
                    modifiedMemoryStream.Position = 0;
                    string fileName = $"{contract.ContractIdNumber}.docx";
                    contract.AzureLink = await _azureBlobService.UploadDocumentFromMemoryStreamAsync(modifiedMemoryStream, fileName);
                }
                if (signingMethod == SettingsValue.SignNow)
                {
                    await _signNowService.AuthenticateAsync();
                    contract.SignNowDocumentId = await _signNowService.UploadInvestmentContractToSignNowAsync(contract, investor, leader, userInProject.Project, shareEquity, disbursementList);
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return contractEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tạo hợp đồng: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        
        public async Task<Contract> CreateStartupShareAllMemberContract(string userId, string projectId, GroupContractCreateDTO groupContractCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var existingContract = await _contractRepository.QueryHelper()
                .Filter(x=>x.ContractType == ContractTypeEnum.INTERNAL 
                && x.ProjectId.Equals(projectId)
                && (x.ContractStatus != ContractStatusEnum.CANCELLED 
                && x.ContractStatus != ContractStatusEnum.EXPIRED) && x.DeletedTime == null).GetOneAsync();
            if (existingContract != null)
            {
                throw new ExistedRecordException(MessageConstant.InternalContractExisted);
            }
            if (groupContractCreateDTO.ShareEquitiesOfMembers == null)
            {
                throw new InvalidDataException(MessageConstant.ShareDistributionListEmptyInContract);
            }
            if (groupContractCreateDTO.ShareEquitiesOfMembers.Count() < 2)
            {
                throw new InvalidDataException(MessageConstant.ShareDistributionMustBeFrom2Member);
            }
            var project = await _projectRepository.GetProjectById(projectId);
            var totalShareOfDistribution = groupContractCreateDTO.ShareEquitiesOfMembers.Sum(d => d.Percentage);
            if (totalShareOfDistribution > project.RemainingPercentOfShares)
            {
                throw new InvalidOperationException(MessageConstant.TotalDistributePercentageGreaterThanRemainingPercentage);
            }
            foreach (var shareEquityOfAMember in groupContractCreateDTO.ShareEquitiesOfMembers)
            {
                var userMemberInContract = await _userManager.FindByIdAsync(shareEquityOfAMember.UserId);
                if (userMemberInContract == null)
                {
                    throw new NotFoundException(MessageConstant.NotFoundUserError + $" {userMemberInContract.Id}");
                    break;
                }
                if (string.IsNullOrWhiteSpace(userMemberInContract.Address) || string.IsNullOrWhiteSpace(userMemberInContract.PhoneNumber)
                || string.IsNullOrWhiteSpace(userMemberInContract.IdCardNumber))
                {
                    throw new InvalidInputException(MessageConstant.InvalidInformationOfUser + $" {userMemberInContract.FullName}");
                    break;
                }

            }
            var leader = userInProject.User;
            if (string.IsNullOrWhiteSpace(leader.Address) || string.IsNullOrWhiteSpace(leader.PhoneNumber)
                || string.IsNullOrWhiteSpace(leader.IdCardNumber))
            {
                throw new InvalidInputException(MessageConstant.InvalidInformationOfUser + $" {leader.FullName}");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                string prefix = "HDNB";
                string currentDateTime = DateTimeOffset.UtcNow.AddHours(7).ToString("ddMMyyyyHHmm");
                string contractIdNumberGen = $"{prefix}-{currentDateTime}";
                Contract contract = new Contract
                {
                    ContractName = groupContractCreateDTO.Contract.ContractName,
                    ContractPolicy = groupContractCreateDTO.Contract.ContractPolicy,
                    ContractType = ContractTypeEnum.INTERNAL,
                    CreatedBy = userInProject.User.FullName,
                    ProjectId = projectId,
                    ContractStatus = ContractStatusEnum.DRAFT,
                    ContractIdNumber = contractIdNumberGen,
                    ExpiredDate = groupContractCreateDTO.Contract.ExpiredDate
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
                        Role = userMemberInContract.Id == leader.Id ? RoleInContract.CREATOR : RoleInContract.SIGNER
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
                contract.UserContracts = usersInContract;
                var contractEntity = _contractRepository.Add(contract);
                await _shareEquityRepository.AddRangeAsync(shareEquitiesOfMembers);
                var signingMethod = await _appSettingManager.GetSettingAsync("SignatureType");
                if (signingMethod == SettingsValue.InternalApp)
                {
                    var modifiedMemoryStream = await _documentFormatService.ReplacePlaceHolderForStartUpShareDistributionDocumentAsync(contract,leader, userInProject.Project, shareEquitiesOfMembers,usersInContract);
                    modifiedMemoryStream.Position = 0;
                    string fileName = $"{contract.ContractIdNumber}.docx";
                    contract.AzureLink = await _azureBlobService.UploadDocumentFromMemoryStreamAsync(modifiedMemoryStream, fileName);
                }
                if (signingMethod == SettingsValue.SignNow)
                {
                    await _signNowService.AuthenticateAsync();
                    contract.SignNowDocumentId = await _signNowService.UploadStartUpShareDistributionContractToSignNowAsync(contract, leader, userInProject.Project, shareEquitiesOfMembers, usersInContract);
                }
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return contractEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tạo hợp đồng: {ex.Message}");
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
            var investor = await _userManager.FindByIdAsync(chosenDeal.InvestorId);
            if (investor == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundInvestorError);
            }
            if (string.IsNullOrWhiteSpace(investor.Address) || string.IsNullOrWhiteSpace(investor.PhoneNumber)
               || string.IsNullOrWhiteSpace(investor.IdCardNumber))
            {
                throw new InvalidInputException(MessageConstant.InvalidInformationOfUser + $" {investor.FullName}");
            }
            var leader = userInProject.User;
            if (string.IsNullOrWhiteSpace(leader.Address) || string.IsNullOrWhiteSpace(leader.PhoneNumber)
                || string.IsNullOrWhiteSpace(leader.IdCardNumber))
            {
                throw new InvalidInputException(MessageConstant.InvalidInformationOfUser + $" {leader.FullName}");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                string prefix = "HDDT";
                string currentDateTime = DateTimeOffset.UtcNow.AddHours(7).ToString("ddMMyyyyHHmm");
                string contractIdNumberGen = $"{prefix}-{currentDateTime}";
                Contract contract = new Contract
                {
                    ContractName = investmentContractFromDealCreateDTO.Contract.ContractName,
                    ContractPolicy = investmentContractFromDealCreateDTO.Contract.ContractPolicy,
                    ContractType = ContractTypeEnum.INVESTMENT,
                    CreatedBy = userInProject.User.FullName,
                    ProjectId = projectId,
                    ContractStatus = ContractStatusEnum.DRAFT,
                    ContractIdNumber = contractIdNumberGen,
                    DealOffer = chosenDeal,
                    DealOfferId = chosenDeal.Id,
                    ExpiredDate = investmentContractFromDealCreateDTO.Contract.ExpiredDate
                };
                List<UserContract> usersInContract = new List<UserContract>();
                List<User> chosenUsersList = new List<User> { investor, leader };
                foreach (var chosenUser in chosenUsersList)
                {
                    UserContract userContract = new UserContract
                    {
                        ContractId = contract.Id,
                        UserId = chosenUser.Id,
                        Role = chosenUser.Id == leader.Id ? RoleInContract.CREATOR : RoleInContract.SIGNER
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
                var disbursementList = await _disbursementRepository.QueryHelper()
                    .Filter(x => x.DealOfferId.Equals(investmentContractFromDealCreateDTO.DealId))
                    .OrderBy(x=>x.OrderBy(x=>x.StartDate))
                    .GetAllAsync();
                foreach (var disbursementTime in disbursementList)
                {
                    disbursementTime.ContractId = contractEntity.Id;
                    _disbursementRepository.Update(disbursementTime);
                }
                var signingMethod = await _appSettingManager.GetSettingAsync("SignatureType");
                if (signingMethod == SettingsValue.InternalApp)
                {
                    var modifiedMemoryStream = await _documentFormatService.ReplacePlaceHolderForInvestmentDocumentAsync(contract, investor, leader, project, shareEquity, disbursementList.ToList());
                    modifiedMemoryStream.Position = 0;
                    string fileName = $"{contract.ContractIdNumber}.docx";
                    contract.AzureLink = await _azureBlobService.UploadDocumentFromMemoryStreamAsync(modifiedMemoryStream, fileName);
                }
                if (signingMethod == SettingsValue.SignNow)
                {
                    await _signNowService.AuthenticateAsync();
                    contract.SignNowDocumentId = await _signNowService.UploadInvestmentContractToSignNowAsync(contract, investor, leader, userInProject.Project, shareEquity, disbursementList.ToList());
                }
                chosenDeal.DealStatus = DealStatusEnum.ContractCreated;
                _dealOfferRepository.Update(chosenDeal);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return contract;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tạo hợp đồng: {ex.Message}");
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
                if (contract.SignNowDocumentId != null)
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
                    if (contract.ContractType == ContractTypeEnum.LIQUIDATIONNOTE)
                    {
                        var webhookMarkExpiredContract = new SignNowWebhookCreateDTO
                        {
                            Action = SignNowServiceConstant.CallBackAction,
                            CallBackUrl = $"{_apiDomain}/api/projects/{projectId}/contracts/{contract.ParentContractId}/expiration",
                            EntityId = userInChosenContract.Contract.SignNowDocumentId,
                            Event = SignNowServiceConstant.DocumentCompleteEvent,
                        };
                        webHookCreateList.Add(webhookMarkExpiredContract);
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
                    webHookCreateList.AddRange(new List<SignNowWebhookCreateDTO> { webhookCompleteSign, webhookUpdate });
                    await _signNowService.RegisterManyWebhookAsync(webHookCreateList);
                    var inviteResponse = await _signNowService.CreateFreeFormInvite(userInChosenContract.Contract.SignNowDocumentId, userEmails);
                }
                userInChosenContract.Contract.LastUpdatedBy = userInChosenContract.User.FullName;
                userInChosenContract.Contract.LastUpdatedTime = DateTimeOffset.UtcNow;
                userInChosenContract.Contract.ContractStatus = ContractStatusEnum.SENT;
                userInChosenContract.Contract.SignDeadline = DateTimeOffset.UtcNow.AddDays(7);
                _contractRepository.Update(userInChosenContract.Contract);
                await _unitOfWork.SaveChangesAsync();
                return userInChosenContract.Contract;
            }

            catch (Exception ex)
            {
                _logger.LogError($"Lỗi gửi ký: {ex.Message}");
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
            if (chosenContract.ContractStatus == ContractStatusEnum.CANCELLED)
            {
                throw new UpdateException(MessageConstant.ThisContractHasBeenCancelled);
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
                throw new InvalidOperationException("Không tìm thấy lời mời ký.");
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
                        throw new InvalidOperationException($"Không tìm thấy người dùng ký: {signedData.Email}");
                    }

                    if (!existingUserContract.SignedDate.HasValue)
                    {
                        // Update the signed date
                        existingUserContract.SignedDate = DateTimeOffset.UtcNow;
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
            if (chosenContract.ContractStatus == ContractStatusEnum.CANCELLED)
            {
                throw new UpdateException(MessageConstant.ThisContractHasBeenCancelled);
            }
            if (projectId != chosenContract.ProjectId)
            {
                throw new UnmatchedException(MessageConstant.ContractNotBelongToProjectError);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                chosenContract.ValidDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddHours(7).Date);
                chosenContract.ContractStatus = ContractStatusEnum.COMPLETED;
                foreach (var equityShare in chosenContract.ShareEquities)
                {
                    equityShare.DateAssigned = DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddHours(7).Date);
                    _shareEquityRepository.Update(equityShare);
                }
                var totalEquitiesSharePercentageInContract = chosenContract.ShareEquities.Sum(e => e.Percentage);
                project.RemainingPercentOfShares = (decimal)(project.RemainingPercentOfShares - totalEquitiesSharePercentageInContract);
                foreach (var disbursement in chosenContract.Disbursements)
                {
                    disbursement.IsValidWithContract = true;
                    _disbursementRepository.Update(disbursement);
                }
                if (chosenContract.DealOffer != null)
                {
                    if (chosenContract.DealOffer.InvestmentCallId != null)
                    {
                        var investmentCall = await _investmentCallService.GetInvestmentCallById(projectId, chosenContract.DealOffer.InvestmentCallId);
                        investmentCall.AmountRaised += chosenContract.DealOffer.Amount;
                        investmentCall.RemainAvailableEquityShare -= chosenContract.DealOffer.EquityShareOffer;
                        investmentCall.TotalInvestor++;
                        if (investmentCall.RemainAvailableEquityShare == 0)
                        {
                            investmentCall.Status = InvestmentCallStatus.Closed;
                            project.ActiveCallId = null;
                            _projectRepository.Update(project);
                        }
                        _investmentCallRepository.Update(investmentCall);
                    }
                }

                var totalDisbursement = chosenContract.Disbursements.Sum(e => e.Amount);
                var projectFinance = await _financeRepository.QueryHelper()
                    .Filter(x => x.ProjectId.Equals(projectId))
                    .GetOneAsync();
                projectFinance.RemainingDisbursement += totalDisbursement;
                _projectRepository.Update(project);
                _contractRepository.Update(chosenContract);
                _financeRepository.Update(projectFinance);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return chosenContract;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi cập nhật hợp đồng: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<DocumentDownLoadResponseDTO> DownLoadFileContract(string userId, string projectId, string contractId)
        {
            var contract = await _contractRepository.GetContractById(contractId);
            if (contract == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundContractError);
            }
            if (projectId != contract.ProjectId)
            {
                throw new UnmatchedException(MessageConstant.ContractNotBelongToProjectError);
            }

            // Check user permissions
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var userInContract = await _userService.CheckIfUserBelongToContract(userId, contractId);

            // Prepare response DTO
            DocumentDownLoadResponseDTO documentDownLoadResponseDTO = new DocumentDownLoadResponseDTO();

            // Determine download source
            if (!string.IsNullOrEmpty(userInContract.Contract.SignNowDocumentId))
            {
                await _signNowService.AuthenticateAsync();
                documentDownLoadResponseDTO = await _signNowService.DownLoadDocument(userInContract.Contract.SignNowDocumentId);
            }
            else if (!string.IsNullOrEmpty(contract.AzureLink))
            {
                documentDownLoadResponseDTO.DownLoadUrl = contract.AzureLink;
            }
            else
            {
                // If neither document ID nor Azure link exists
                throw new NotFoundException(MessageConstant.NotFoundDocumentError);
            }

            return documentDownLoadResponseDTO;
        }

        public async Task<PaginationDTO<ContractSearchResponseDTO>> SearchContractWithFilters(string userId, string projectId, ContractSearchDTO search, int page, int size)
        {
            var userProject = await _userService.CheckIfUserInProject(userId, projectId);
            var searchResult = _contractRepository.GetContractListQuery(userId, projectId);

            if (!string.IsNullOrWhiteSpace(search.ContractIdNumber))
            {
                string contractIdNumber = search.ContractIdNumber.ToLower();
                searchResult = searchResult.Where(c => c.ContractIdNumber.ToLower().Contains(contractIdNumber));
            }

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
                .Include(c=>c.Disbursements)
                .Include(c => c.UserContracts)
                .ThenInclude(uc => uc.User)
                .OrderByDescending(x => x.LastUpdatedTime)
                .ToListAsync();

            var contractSearchResponseDTOs = pagedResult.Select(contract => new ContractSearchResponseDTO
            {
                Id = contract.Id,
                ContractIdNumber = contract.ContractIdNumber,
                ContractName = contract.ContractName,
                ContractStatus = contract.ContractStatus,
                ContractType = contract.ContractType,
                LastUpdatedTime = contract.LastUpdatedTime,
                LiquidationNoteId = contract.LiquidationNoteId,
                ParentContractId = contract.ParentContractId,
                CurrentTerminationRequestId = contract.CurrentTerminationRequestId,
                Parties = contract.UserContracts.Select(userContract => new UserInContractResponseDTO
                {
                    Id = userContract.User.Id,
                    Email = userContract.User.Email,
                    FullName = userContract.User.FullName,
                    PhoneNumber = userContract.User.PhoneNumber,
                    ProfilePicture = userContract.User.ProfilePicture
                }).ToList(),
                TotalDisbursementAmount = contract.Disbursements.Sum(d => d.Amount),
                DisbursedAmount = contract.Disbursements.Where(d => d.DisbursementStatus == DisbursementStatusEnum.FINISHED).Sum(d => d.Amount),
                PendingAmount = contract.Disbursements.Where(d => d.DisbursementStatus != DisbursementStatusEnum.FINISHED).Sum(d => d.Amount),
                Disbursements = contract.Disbursements.Select(d => new DisbursementInContractListResponseDTO
                {
                    Id = d.Id,
                    Amount = d.Amount.ToString(),
                    DisbursementStatus = d.DisbursementStatus,
                    StartDate = d.StartDate,
                    EndDate = d.EndDate,
                    Title = d.Title
                }).ToList(),
                ValidDate = contract.ValidDate
            }).ToList();
            foreach (var contractItem in contractSearchResponseDTOs)
            {
                if (contractItem.CurrentTerminationRequestId != null)
                {
                    var request = await _terminationRequestRepository.QueryHelper().Filter(x=>x.Id.Equals(contractItem.CurrentTerminationRequestId))
                        .Include(x => x.Appointment)
                        .GetOneAsync();
                    if (request != null)
                    {
                        if (request.Appointment != null)
                        {
                            contractItem.MeetingStatus = request.Appointment.Status;
                        }
                    }
                }
            }
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
                contract.LastUpdatedTime = DateTimeOffset.UtcNow;
                contract.LastUpdatedBy = userInProject.User.FullName;
                contract.ExpiredDate = investmentContractUpdateDTO.Contract.ExpiredDate;

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
                var disbursementList = new List<Disbursement>();
                if (contract.DealOfferId != null)
                {
                    var existingDisbursements = contract.Disbursements;
                    disbursementList.AddRange(existingDisbursements);
                }

                if (contract.DealOfferId == null)
                {
                    var existingDisbursements = contract.Disbursements;
                    foreach (var existingDisbursement in existingDisbursements)
                    {
                        await _disbursementRepository.DeleteAsync(existingDisbursement);
                    }
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
                }

                // Upload updated contract to SignNow
                var signingMethod = await _appSettingManager.GetSettingAsync("SignatureType");
                if (signingMethod == SettingsValue.InternalApp)
                {
                    var modifiedMemoryStream = await _documentFormatService.ReplacePlaceHolderForInvestmentDocumentAsync(contract, investor, leader, project, shareEquity, disbursementList);
                    modifiedMemoryStream.Position = 0;
                    string fileName = $"{contract.ContractIdNumber}.docx";
                    contract.AzureLink = await _azureBlobService.UploadDocumentFromMemoryStreamAsync(modifiedMemoryStream, fileName);
                }
                if (signingMethod == SettingsValue.SignNow)
                {
                    await _signNowService.AuthenticateAsync();
                    contract.SignNowDocumentId = await _signNowService.UploadInvestmentContractToSignNowAsync(contract, investor, leader, userInProject.Project, shareEquity, disbursementList);
                }

                var contractEntity = _contractRepository.Update(contract);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return contractEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi cập nhật");
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
            var project = await _projectRepository.GetProjectById(projectId);
            var totalShareOfDistribution = groupContractUpdateDTO.ShareEquitiesOfMembers.Sum(d => d.Percentage);
            if (totalShareOfDistribution > project.RemainingPercentOfShares)
            {
                throw new InvalidOperationException(MessageConstant.TotalDistributePercentageGreaterThanRemainingPercentage);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var leader = userInProject.User;
                // Update contract details
                contract.ContractName = groupContractUpdateDTO.Contract.ContractName;
                contract.ContractPolicy = groupContractUpdateDTO.Contract.ContractPolicy;
                contract.LastUpdatedTime = DateTimeOffset.UtcNow;
                contract.LastUpdatedBy = userInProject.User.FullName;
                contract.ExpiredDate = groupContractUpdateDTO.Contract.ExpiredDate;

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
                        Role = userMemberInContract.Id == leader.Id ? RoleInContract.CREATOR : RoleInContract.SIGNER
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
                
                contract.UserContracts = usersInContract;
                var contractEntity = _contractRepository.Update(contract);
                await _shareEquityRepository.AddRangeAsync(shareEquitiesOfMembers);
                var signingMethod = await _appSettingManager.GetSettingAsync("SignatureType");
                if (signingMethod == SettingsValue.InternalApp)
                {
                    var modifiedMemoryStream = await _documentFormatService.ReplacePlaceHolderForStartUpShareDistributionDocumentAsync(contract, leader, userInProject.Project, shareEquitiesOfMembers, usersInContract);
                    modifiedMemoryStream.Position = 0;
                    string fileName = $"{contract.ContractIdNumber}.docx";
                    contract.AzureLink = await _azureBlobService.UploadDocumentFromMemoryStreamAsync(modifiedMemoryStream, fileName);
                }
                if (signingMethod == SettingsValue.SignNow)
                {
                    await _signNowService.AuthenticateAsync();
                    contract.SignNowDocumentId = await _signNowService.UploadStartUpShareDistributionContractToSignNowAsync(contract, leader, userInProject.Project, shareEquitiesOfMembers, usersInContract);
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return contractEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi cập nhật.");
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
            if (chosenContract.ContractStatus != ContractStatusEnum.SENT)
            {
                throw new UpdateException(MessageConstant.CannotCancelContractError);
            }
            try {
                _unitOfWork.BeginTransaction();
                if (chosenContract.ContractType == ContractTypeEnum.LIQUIDATIONNOTE)
                {
                    var parentContract = await _contractRepository.GetContractById(chosenContract.ParentContractId);
                    parentContract.LiquidationNoteId = null;
                    parentContract.ContractStatus = ContractStatusEnum.COMPLETED;
                    _contractRepository.Update(parentContract);
                }
                if (chosenContract.ContractType == ContractTypeEnum.INVESTMENT) 
                {
                    if (chosenContract.Disbursements != null)
                    {
                        decimal totalPendingAmount = 0;
                        foreach (var disbursement in chosenContract.Disbursements)
                        {
                            if (disbursement.DisbursementStatus == DisbursementStatusEnum.PENDING)
                            {
                                totalPendingAmount += disbursement.Amount; // Sum pending disbursement amounts
                                disbursement.IsValidWithContract = false;
                                disbursement.DisbursementStatus = DisbursementStatusEnum.CANCELLED;
                                _disbursementRepository.Update(disbursement);
                            }
                        }
                    }
                }  
                chosenContract.ContractStatus = ContractStatusEnum.CANCELLED;
                userInContract.IsReject = true;
                _contractRepository.Update(chosenContract);
                await _contractRepository.UpdateUserInContract(userInContract);
                if (chosenContract.DealOfferId != null) 
                {
                    var dealOffer = await _dealOfferRepository.QueryHelper()
                        .Filter(x => x.Id.Equals(chosenContract.DealOfferId))
                        .GetOneAsync();
                    dealOffer.DealStatus = DealStatusEnum.Rejected;
                    _dealOfferRepository.Update(dealOffer);
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return chosenContract;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi cập nhật");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<Contract> CreateLiquidationNote(string userId, string projectId, string contractId, IFormFile uploadFile)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }

            var chosenContract = await _contractRepository.GetContractById(contractId);

            // Kiểm tra nếu hợp đồng đã hết hạn hoặc đang chờ thanh lý
            if (chosenContract.ContractStatus != ContractStatusEnum.WAITINGFORLIQUIDATION)
            {
                throw new InvalidDataException(MessageConstant.ThisContractIsNotInLiquidatedState);
            }

            if (chosenContract.ContractType == ContractTypeEnum.LIQUIDATIONNOTE)
            {
                throw new InvalidDataException(MessageConstant.CannotCancelContractError);
            }

            if (chosenContract.CurrentTerminationRequestId == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTerminateRequest);
            }

            var request = await _terminationRequestRepository.GetOneAsync(chosenContract.CurrentTerminationRequestId);
            if (request.IsAgreed != true)
            {
                throw new InvalidDataException(MessageConstant.CannotCancelContractError);
            }
            if (request.AppointmentId == null)
            {
                throw new InvalidDataException(MessageConstant.MeetingNotFound);
            }
            var appointment = await _appointmentRepository.QueryHelper()
                .Include(x => x.MeetingNotes)
                .Filter(x => x.Id.Equals(request.AppointmentId) && x.Status == MeetingStatus.Finished)
                .GetOneAsync();
            if (appointment == null)
            {
                throw new InvalidDataException(MessageConstant.MeetingIsNotFinished);
            }
            if (appointment.MeetingNotes == null)
            {
                throw new InvalidDataException(MessageConstant.NotFoundMeetingNote);
            }

            var existingProcessingLiquidation = await _contractRepository.QueryHelper()
                .Filter(x => x.ParentContractId == chosenContract.Id
                             && ((x.ContractStatus == ContractStatusEnum.DRAFT && x.DeletedTime == null) 
                             || x.ContractStatus == ContractStatusEnum.SENT))
                .GetOneAsync();

            if (existingProcessingLiquidation != null)
            {
                throw new InvalidDataException(MessageConstant.ExistingProcessingLiquidationError);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                //if (request.FromId != userId) 
                //{
                //    string prefix = "GTL";
                //    string currentDateTime = DateTimeOffset.UtcNow.AddHours(7).ToString("ddMMyyyyHHmm"); ;
                //    string contractIdNumberGen = $"{prefix}-{currentDateTime}";
                //    Contract liquidationNote = new Contract
                //    {
                //        ContractName = $"Biên bản thanh lý cho hợp đồng {chosenContract.ContractIdNumber}",
                //        ContractPolicy = $"Thanh lý cho hợp đồng {chosenContract.ContractIdNumber}",
                //        ContractType = ContractTypeEnum.LIQUIDATIONNOTE,
                //        CreatedBy = userInProject.User.FullName,
                //        ProjectId = projectId,
                //        ContractStatus = ContractStatusEnum.DRAFT,
                //        ContractIdNumber = contractIdNumberGen,
                //        ParentContractId = chosenContract.Id,
                //        ParentContract = chosenContract.ParentContract,
                //    };

                //    var leader = userInProject.User;
                //    var leaderInLiquidationNote = new UserContract
                //    {
                //        UserId = leader.Id,
                //        ContractId = liquidationNote.Id,
                //        Role = RoleInContract.CREATOR
                //    };


                //    var requestParty = new UserContract
                //    {
                //        UserId = request.FromId,
                //        ContractId = liquidationNote.Id,
                //        Role = RoleInContract.SIGNER,
                //    };

                //    List<UserContract> usersInContract = new List<UserContract> { leaderInLiquidationNote, requestParty };

                //    liquidationNote.UserContracts = usersInContract;
                //    var liquidationEntity = _contractRepository.Add(liquidationNote);

                //    chosenContract.LiquidationNoteId = liquidationEntity.Id;
                //    chosenContract.LastUpdatedBy = userInProject.User.FullName;
                //    chosenContract.LastUpdatedTime = DateTimeOffset.UtcNow;
                //    _contractRepository.Update(chosenContract);

                //    var signingMethod = await _appSettingManager.GetSettingAsync("SignatureType");
                //    if (signingMethod == SettingsValue.InternalApp)
                //    {
                //        liquidationNote.AzureLink = await _azureBlobService.UploadLiquidationNote(uploadFile);
                //    }
                //    if (signingMethod == SettingsValue.SignNow)
                //    {
                //        await _signNowService.AuthenticateAsync();
                //        liquidationNote.SignNowDocumentId = await _signNowService.UploadDocumentAsync(uploadFile);
                //    }
                //    await _unitOfWork.SaveChangesAsync();
                //    await _unitOfWork.CommitAsync();
                //    return liquidationEntity;
                //}

                if (chosenContract.ContractType == ContractTypeEnum.INVESTMENT)
                {
                    string prefix = "GTL";
                    string currentDateTime = DateTimeOffset.UtcNow.AddHours(7).ToString("ddMMyyyyHHmm"); ;
                    string contractIdNumberGen = $"{prefix}-{currentDateTime}";
                    Contract liquidationNote = new Contract
                    {
                        ContractName = $"Biên bản thanh lý cho hợp đồng {chosenContract.ContractIdNumber}",
                        ContractPolicy = $"Thanh lý cho hợp đồng {chosenContract.ContractIdNumber}",
                        ContractType = ContractTypeEnum.LIQUIDATIONNOTE,
                        CreatedBy = userInProject.User.FullName,
                        ProjectId = projectId,
                        ContractStatus = ContractStatusEnum.DRAFT,
                        ContractIdNumber = contractIdNumberGen,
                        ParentContractId = chosenContract.Id,
                        ParentContract = chosenContract.ParentContract,
                    };

                    List<UserContract> usersInContract = new List<UserContract>();

                    foreach (var userParty in chosenContract.UserContracts)
                    {
                        UserContract userInLiquidation = new UserContract
                        {
                            Contract = liquidationNote,
                            ContractId = liquidationNote.Id,
                            UserId = userParty.UserId,
                            User = userParty.User,
                            Role = userParty.UserId == userInProject.UserId ? RoleInContract.CREATOR : RoleInContract.SIGNER
                        };
                        usersInContract.Add(userInLiquidation);
                    }

                   

                    liquidationNote.UserContracts = usersInContract;
                    var liquidationEntity = _contractRepository.Add(liquidationNote);

                    chosenContract.LiquidationNoteId = liquidationEntity.Id;
                    chosenContract.LastUpdatedBy = userInProject.User.FullName;
                    chosenContract.LastUpdatedTime = DateTimeOffset.UtcNow;
                    _contractRepository.Update(chosenContract);

                    var signingMethod = await _appSettingManager.GetSettingAsync("SignatureType");
                    if (signingMethod == SettingsValue.InternalApp)
                    {
                        liquidationNote.AzureLink = await _azureBlobService.UploadLiquidationNote(uploadFile);
                    }
                    if (signingMethod == SettingsValue.SignNow)
                    {
                        await _signNowService.AuthenticateAsync();
                        liquidationNote.SignNowDocumentId = await _signNowService.UploadDocumentAsync(uploadFile);
                    }
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                    return liquidationEntity;
                }

                else
                {
                    var project = await _projectRepository.GetProjectById(projectId);
                    chosenContract.ExpiredDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddHours(7).Date);
                    chosenContract.ContractStatus = ContractStatusEnum.EXPIRED;
                    chosenContract.LastUpdatedTime = DateTimeOffset.UtcNow;
                    chosenContract.LastUpdatedBy = userInProject.User.FullName;
                    if (chosenContract.ContractType == ContractTypeEnum.INTERNAL)
                    {
                        decimal expiredContractShare = (decimal)chosenContract.ShareEquities.Sum(x => x.Percentage);
                        project.RemainingPercentOfShares += expiredContractShare;
                        _logger.LogInformation($"Hợp đồng nội bộ hết hạn. Trả lại {expiredContractShare}% cổ phần cho dự án {project.ProjectName}. Tổng cổ phần còn lại: {project.RemainingPercentOfShares}%.");
                        _projectRepository.Update(project);
                    }
                    if (chosenContract.ContractType == ContractTypeEnum.INVESTMENT)
                    {
                        // Xử lý khi hợp đồng đầu tư hết hạn
                        decimal investmentShare = (decimal)chosenContract.ShareEquities.Sum(x => x.Percentage);

                        // Logic tuỳ chọn: Mua lại, hoàn trả, hoặc chuyển nhượng cổ phần
                        project.RemainingPercentOfShares += investmentShare;
                        _logger.LogInformation($"Hợp đồng đầu tư hết hạn. Trả lại {investmentShare}% cổ phần cho dự án {project.ProjectName}. Tổng cổ phần còn lại: {project.RemainingPercentOfShares}%.");
                        _projectRepository.Update(project);
                        decimal totalPendingAmount = 0;
                        if (chosenContract.Disbursements != null)
                        {
                            foreach (var disbursement in chosenContract.Disbursements)
                            {
                                if (disbursement.DisbursementStatus == DisbursementStatusEnum.PENDING)
                                {
                                    totalPendingAmount += disbursement.Amount; // Sum pending disbursement amounts
                                    disbursement.IsValidWithContract = false;
                                    disbursement.DisbursementStatus = DisbursementStatusEnum.CANCELLED;
                                    _disbursementRepository.Update(disbursement);
                                }
                            }
                        }
                        project.Finance.RemainingDisbursement -= totalPendingAmount;
                        _financeRepository.Update(project.Finance);
                        _logger.LogInformation($"Tổng số tiền chưa giải ngân bị tắt: {totalPendingAmount}.");
                    }
                    string prefix = "GTL";
                    string currentDateTime = DateTimeOffset.UtcNow.AddHours(7).ToString("ddMMyyyyHHmm");
                    string contractIdNumberGen = $"{prefix}-{currentDateTime}";
                    Contract liquidationNote = new Contract
                    {
                        ContractName = $"Biên bản thanh lý cho hợp đồng {chosenContract.ContractIdNumber}",
                        ContractPolicy = $"Thanh lý cho hợp đồng {chosenContract.ContractIdNumber}",
                        ContractType = ContractTypeEnum.LIQUIDATIONNOTE,
                        CreatedBy = userInProject.User.FullName,
                        ProjectId = projectId,
                        ContractStatus = ContractStatusEnum.COMPLETED,
                        ContractIdNumber = contractIdNumberGen,
                        ParentContractId = chosenContract.Id,
                        ParentContract = chosenContract.ParentContract,
                        ValidDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date)
                    };

                    List<UserContract> usersInLiquidationNote = new List<UserContract>();
                    foreach (var userParty in chosenContract.UserContracts)
                    {
                        var userInLiquidation = new UserContract
                        {
                            UserId = userParty.UserId,
                            ContractId = liquidationNote.Id,
                            IsReject = false,
                            SignedDate = DateTimeOffset.UtcNow,
                            Role = userParty.Role,
                        };
                        usersInLiquidationNote.Add(userInLiquidation);

                    }

                    liquidationNote.UserContracts = usersInLiquidationNote;
                    var liquidationEntity = _contractRepository.Add(liquidationNote);

                    chosenContract.LiquidationNoteId = liquidationEntity.Id;
                    chosenContract.LastUpdatedBy = userInProject.User.FullName;
                    chosenContract.LastUpdatedTime = DateTimeOffset.UtcNow;
                    _contractRepository.Update(chosenContract);

                    var signingMethod = await _appSettingManager.GetSettingAsync("SignatureType");
                    if (signingMethod == SettingsValue.InternalApp)
                    {
                        liquidationNote.AzureLink = await _azureBlobService.UploadLiquidationNote(uploadFile);
                    }
                    if (signingMethod == SettingsValue.SignNow)
                    {
                        await _signNowService.AuthenticateAsync();
                        liquidationNote.SignNowDocumentId = await _signNowService.UploadDocumentAsync(uploadFile);
                    }
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                    return liquidationEntity;
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tạo hợp đồng: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task MarkExpiredContract(string projectId, string contractId)
        {
            var contract = await _contractRepository.GetContractById(contractId);
            if (contract.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.ContractNotBelongToProjectError);
            }
            if (contract.ContractStatus != ContractStatusEnum.COMPLETED && contract.ContractStatus != ContractStatusEnum.WAITINGFORLIQUIDATION)
            {
                throw new UpdateException(MessageConstant.ContractIsNotValid);
            }
            var project = await _projectRepository.GetProjectById(projectId);
            if (project == null) 
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }

            try
            {
                _unitOfWork.BeginTransaction();
                contract.ExpiredDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddHours(7).Date);
                contract.ContractStatus = ContractStatusEnum.EXPIRED;
                contract.LastUpdatedTime = DateTimeOffset.UtcNow;
                if (contract.ContractType == ContractTypeEnum.INTERNAL)
                {
                    decimal expiredContractShare = (decimal)contract.ShareEquities.Sum(x => x.Percentage);
                    project.RemainingPercentOfShares += expiredContractShare;
                    _logger.LogInformation($"Hợp đồng nội bộ hết hạn. Trả lại {expiredContractShare}% cổ phần cho dự án {project.ProjectName}. Tổng cổ phần còn lại: {project.RemainingPercentOfShares}%.");
                    _projectRepository.Update(project);
                }
                else if (contract.ContractType == ContractTypeEnum.INVESTMENT)
                {
                    // Xử lý khi hợp đồng đầu tư hết hạn
                    decimal investmentShare = (decimal)contract.ShareEquities.Sum(x => x.Percentage);

                    // Logic tuỳ chọn: Mua lại, hoàn trả, hoặc chuyển nhượng cổ phần
                    project.RemainingPercentOfShares += investmentShare;
                    _logger.LogInformation($"Hợp đồng đầu tư hết hạn. Trả lại {investmentShare}% cổ phần cho dự án {project.ProjectName}. Tổng cổ phần còn lại: {project.RemainingPercentOfShares}%.");
                    _projectRepository.Update(project);
                    decimal totalPendingAmount = 0;
                    if (contract.Disbursements != null)
                    {
                        foreach (var disbursement in contract.Disbursements)
                        {
                            if (disbursement.DisbursementStatus == DisbursementStatusEnum.PENDING)
                            {
                                totalPendingAmount += disbursement.Amount; // Sum pending disbursement amounts
                                disbursement.IsValidWithContract = false;
                                disbursement.DisbursementStatus = DisbursementStatusEnum.CANCELLED;
                                _disbursementRepository.Update(disbursement);
                            }
                        }
                    }
                    project.Finance.RemainingDisbursement -= totalPendingAmount;
                    _financeRepository.Update(project.Finance);
                    _logger.LogInformation($"Tổng số tiền chưa giải ngân bị tắt: {totalPendingAmount}.");
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

        public async Task CreateMeetingForLeaderTerminationContract(string userId, string projectId, string contractId, TerminationMeetingCreateDTO terminationMeetingCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var userInContract = await _userService.CheckIfUserBelongToContract(userId, contractId);
            var chosenContract = await _contractRepository.GetContractById(contractId);
            if (chosenContract.ContractStatus != ContractStatusEnum.COMPLETED && chosenContract.ContractStatus != ContractStatusEnum.WAITINGFORLIQUIDATION)
            {
                throw new UpdateException(MessageConstant.ContractIsNotValid);
            }

            if (chosenContract.ContractType == ContractTypeEnum.LIQUIDATIONNOTE)
            {
                throw new InvalidDataException(MessageConstant.CannotCancelContractError);
            }

            var existingRequest = await _terminationRequestRepository.QueryHelper()
                .Filter(x => x.ContractId == contractId
                && x.FromId == userId
                && x.IsAgreed == null)
                .GetOneAsync();

            var otherExistingRequest = await _terminationRequestRepository.QueryHelper()
                .Filter(x => x.ContractId == contractId
                && x.IsAgreed == null)
                .GetOneAsync();

            if (userInContract.Contract.ContractType == CrossCutting.Enum.ContractTypeEnum.LIQUIDATIONNOTE)
            {
                throw new InvalidDataException(MessageConstant.YouCannotRequestToTerminateThisContract);
            }
            if (userInContract.Contract.ContractStatus != CrossCutting.Enum.ContractStatusEnum.COMPLETED ||
                existingRequest != null ||
                otherExistingRequest != null)
            {
                throw new InvalidDataException(MessageConstant.YouCannotRequestToTerminateThisContract);
            }

            try
            {
                _unitOfWork.BeginTransaction();
                var project = await _projectRepository.GetProjectById(projectId);

                Appointment appointment = new Appointment
                {
                    AppointmentTime = terminationMeetingCreateDTO.AppointmentTime,
                    CreatedBy = userInProject.User.FullName,
                    Description = terminationMeetingCreateDTO.Description,
                    MeetingLink = terminationMeetingCreateDTO.MeetingLink,
                    Title = terminationMeetingCreateDTO.Title,
                    Status = CrossCutting.Enum.MeetingStatus.Proposed,
                    ProjectId = projectId,
                    Project = userInProject.Project,
                    ContractId = contractId
                };

                var newMeeting = _appointmentRepository.Add(appointment);
                var terminationRequest = new TerminationRequest
                {
                    ContractId = contractId,
                    Contract = chosenContract,
                    CreatedBy = userInProject.User.FullName,
                    FromId = userInProject.User.Id,
                    Reason = "Huỷ hợp đồng",
                    IsAgreed = true,
                    Appointment = appointment,
                    AppointmentId = appointment.Id
                };

                await _appointmentRepository.AddUserToAppointment(userId, newMeeting.Id);

                var leaderRequest = _terminationRequestRepository.Add(terminationRequest);

                chosenContract.CurrentTerminationRequestId = leaderRequest.Id;
                chosenContract.ContractStatus = ContractStatusEnum.WAITINGFORLIQUIDATION;
                chosenContract.LastUpdatedBy = userInProject.User.FullName;
                chosenContract.LastUpdatedTime = DateTimeOffset.UtcNow;
                _contractRepository.Update(chosenContract);

                foreach (var userParty in chosenContract.UserContracts.Where(uc => uc.UserId != userId))
                {
                    await _emailService.SendAppointmentInvite(userParty.User.Email, userInProject.Project.ProjectName, userParty.User.FullName, newMeeting.MeetingLink, newMeeting.AppointmentTime.AddHours(7));
                    await _appointmentRepository.AddUserToAppointment(userParty.UserId, newMeeting.Id);
                }


                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MessageConstant.UpdateFailed);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task CancelLiquidationAfterMeeting(string userId, string projectId, string contractId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var terminatedContract = await _contractRepository.GetContractById(contractId);
            if (terminatedContract.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.ContractNotBelongToProjectError);
            }
            var userInContract = await _userService.CheckIfUserBelongToContract(userId, contractId);
            if (terminatedContract.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.ContractNotBelongToProjectError);
            }

            if (terminatedContract.ContractStatus != ContractStatusEnum.WAITINGFORLIQUIDATION)
            {
                throw new InvalidDataException(MessageConstant.ThisContractIsNotInLiquidatedState);
            }

            if (terminatedContract.ContractType == ContractTypeEnum.LIQUIDATIONNOTE)
            {
                throw new InvalidDataException(MessageConstant.CannotCancelContractError);
            }

            if (terminatedContract.CurrentTerminationRequestId != null)
            {
                var request = await _terminationRequestRepository.GetOneAsync(terminatedContract.CurrentTerminationRequestId);
                if (request.IsAgreed != true)
                {
                    throw new InvalidDataException(MessageConstant.CannotCancelContractError);
                }
                if (request.AppointmentId == null)
                {
                    throw new InvalidDataException(MessageConstant.MeetingNotFound);
                }
                var appointment = await _appointmentRepository.QueryHelper()
                .Include(x => x.MeetingNotes)
                .Filter(x => x.Id.Equals(request.AppointmentId) && x.Status == MeetingStatus.Finished)
                .GetOneAsync();
                if (appointment == null)
                {
                    throw new InvalidDataException(MessageConstant.MeetingIsNotFinished);
                }
                if (appointment.MeetingNotes == null)
                {
                    throw new InvalidDataException(MessageConstant.NotFoundMeetingNote);
                }
            }

            try
            {
                _unitOfWork.BeginTransaction();
                var project = await _projectRepository.GetProjectById(projectId);
                terminatedContract.ContractStatus = ContractStatusEnum.COMPLETED;
                terminatedContract.LastUpdatedTime = DateTimeOffset.UtcNow;
                terminatedContract.LastUpdatedBy = userInProject.User.FullName;
                terminatedContract.CurrentTerminationRequestId = null;
                if (terminatedContract.LiquidationNoteId != null) 
                {
                    var liquidationNote = await _contractRepository.GetOneAsync(terminatedContract.LiquidationNoteId);
                    if (liquidationNote != null)
                    {
                        liquidationNote.ContractStatus = ContractStatusEnum.CANCELLED;
                        _contractRepository.Update(liquidationNote);
                    }
                }
                _contractRepository.Update(terminatedContract);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex) {
                _logger.LogError(ex, MessageConstant.UpdateFailed);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteContract(string userId, string projectId, string contractId)
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
                if (contract.ContractStatus == ContractStatusEnum.DRAFT)
                {
                    if (contract.ContractType == ContractTypeEnum.LIQUIDATIONNOTE)
                    {
                        var parentContract = await _contractRepository.GetContractById(contract.ParentContractId);
                        parentContract.LiquidationNoteId = null;
                        parentContract.LastUpdatedBy = userInProject.User.FullName;
                        parentContract.LastUpdatedTime = DateTimeOffset.UtcNow;
                        _contractRepository.Update(parentContract);
                    }
                    await _contractRepository.SoftDeleteById(contractId);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception(MessageConstant.DeleteFailed);
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
                if (contract.ContractType == ContractTypeEnum.LIQUIDATIONNOTE)
                {
                    var parentContract = await _contractRepository.GetContractById(contract.ParentContractId);
                    parentContract.LiquidationNoteId = null;
                    parentContract.LastUpdatedTime = DateTimeOffset.UtcNow;
                    _contractRepository.Update(parentContract);
                }
                contract.ContractStatus = ContractStatusEnum.CANCELLED; 
                _contractRepository.Update(contract);
                if (contract.DealOfferId != null)
                {
                    var dealOffer = await _dealOfferRepository.QueryHelper()
                        .Filter(x => x.Id.Equals(contract.DealOfferId))
                        .GetOneAsync();
                    dealOffer.DealStatus = DealStatusEnum.Rejected;
                    dealOffer.LastUpdatedTime = DateTimeOffset.UtcNow;
                    _dealOfferRepository.Update(dealOffer);
                }
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<List<UserContract>> GetUserSignHistoryInAContract(string userId, string projectId, string contractId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var loginUserInContract = await _userService.CheckIfUserBelongToContract(userId, contractId);
            if (userInProject.ProjectId != loginUserInContract.Contract.ProjectId)
            {
                throw new UnmatchedException(MessageConstant.ContractNotBelongToProjectError);
            }
            var contract = await _contractRepository.GetContractById(contractId);
            var usersInContract = contract.UserContracts.ToList();
            return usersInContract;
        }
    }
}
