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
using DocumentFormat.OpenXml.Spreadsheet;
using CrossCutting.Exceptions;
using DocumentFormat.OpenXml.Office2010.Excel;

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
        

        public async Task<Contract> CreateInvestmentContract(string userId,string projectId ,InvestmentContractCreateDTO investmentContractCreateDTO)
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
                        DisbursementStatus = DisbursementStatusEnum.PENDING,
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
                    CallBackUrl = $"{_apiDomain}/api/projects/{projectId}/contracts/valid-contract/{contractId}",
                    EntityId = userInChosenContract.Contract.SignNowDocumentId,
                    Event = SignNowServiceConstant.DocumentCompleteEvent
                };
                await _signNowService.RegisterWebhookAsync(webhookCompleteSign);
                var webhookUpdate = new SignNowWebhookCreateDTO
                {
                    Action = SignNowServiceConstant.CallBackAction,
                    CallBackUrl = $"{_apiDomain}/api/projects/{projectId}/contracts/sign-confirmation/{contractId}",
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

        public async Task<SearchResponseDTO<ContractSearchResponseDTO>> SearchContractWithFilters(string userId, string projectId, ContractSearchDTO search, int pageIndex, int pageSize)
        {
            var userProject = await _userService.CheckIfUserInProject(userId, projectId);
            var searchResult = _contractRepository.QueryHelper().Include(c => c.UserContracts).Filter(x=>x.ProjectId.Equals(projectId) && x.UserContracts.Any(us => us.UserId.Equals(userId)));
            // Filter by Contract Name
            if (!string.IsNullOrWhiteSpace(search.ContractName))
            {
                string contractNameLower = search.ContractName.ToLower();
                searchResult = searchResult.Filter(c => c.ContractName.ToLower().Contains(contractNameLower));
            }

            // Filter by Contract Type Enum
            if (search.ContractTypeEnum.HasValue)
            {
                searchResult = searchResult.Filter(c => c.ContractType == search.ContractTypeEnum.Value);
            }

            // Filter by Parties (User IDs)
            if (search.Parties?.Any() == true)
            {
                var partyIds = search.Parties;

                // Filter contracts that contain all the specified parties
                searchResult = searchResult.Filter(c =>
                    partyIds.All(partyId => c.UserContracts.Any(uc => uc.UserId == partyId)));
            }

            // Filter by Last Updated Date Range
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

            // Filter by Contract Status Enum
            if (search.ContractStatusEnum.HasValue)
            {
                searchResult = searchResult.Filter(c => c.ContractStatus == search.ContractStatusEnum.Value);
            }
            var records = await searchResult.GetAllAsync();
            var totalRecord = records.Count();

            var searchResultList = await searchResult.GetPagingAsync(pageIndex, pageSize);
            
            // Mapping contracts and their users to the DTOs
            foreach (var contract in searchResultList)
            {
                foreach (var userConstract in contract.UserContracts) {
                    var userParty = await _userManager.FindByIdAsync(userConstract.UserId);
                    userConstract.User = userParty;   
                }
            };
            List<ContractSearchResponseDTO> contractSearchResponseDTOs = new List<ContractSearchResponseDTO>();
            foreach (var contract in searchResultList) 
            {
                var usersInContractResponse = new List<UserInContractResponseDTO>();
                foreach (var userConstract in contract.UserContracts)
                {
                    UserInContractResponseDTO user = new UserInContractResponseDTO
                    {
                        Id = userConstract.User.Id,
                        Email = userConstract.User.Email,
                        FullName = userConstract.User.FullName,
                        PhoneNumber = userConstract.User.PhoneNumber
                    };
                    usersInContractResponse.Add(user);

                }

                ContractSearchResponseDTO contractResponseDTO = new ContractSearchResponseDTO
                {
                    Id = contract.Id,
                    ContractName = contract.ContractName,
                    ContractStatus = contract.ContractStatus,
                    ContractType = contract.ContractType,
                    LastUpdatedTime = contract.LastUpdatedTime,
                    Parties = usersInContractResponse
                };
                contractSearchResponseDTOs.Add(contractResponseDTO);
            }

            var response = new SearchResponseDTO<ContractSearchResponseDTO>
            {
                ResponseList = contractSearchResponseDTOs,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRecord = totalRecord,
                TotalPage = (int)Math.Ceiling((double)totalRecord / pageSize)
            };
            return response;
        }

        public async Task<Contract> UpdateInvestmentContract(string userId, string projectId ,string contractId, InvestmentContractUpdateDTO investmentContractUpdateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            var leader = await _userManager.FindByIdAsync(userId);

            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }

            var contract = await _contractRepository.GetContractById(contractId);
            if (contract == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundContractError);
            }
            if (contract.ContractStatus != ContractStatusEnum.DRAFT)
            {
                throw new UpdateException(MessageConstant.CannotUpdateContractError);
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
                    existingShareEquity.ShareQuantity = investmentContractUpdateDTO.InvestorInfo.ShareQuantity;
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
                        ShareQuantity = investmentContractUpdateDTO.InvestorInfo.ShareQuantity,
                        StakeHolderType = RoleInTeam.Investor,
                        CreatedBy = leader.FullName,
                    };
                    _shareEquityRepository.Add(shareEquity);
                }

                // Update disbursements
                var disbursementList = new List<Disbursement>();
                foreach (var updatedDisbursement in investmentContractUpdateDTO.Disbursements)
                {
                    var existingDisbursement = contract.Disbursements.FirstOrDefault(d => d.Id == updatedDisbursement.Id);

                    if (existingDisbursement != null)
                    {
                        // Update fields of the existing disbursement
                        existingDisbursement.Amount = updatedDisbursement.Amount;
                        existingDisbursement.Condition = updatedDisbursement.Condition;
                        existingDisbursement.StartDate = updatedDisbursement.StartDate;
                        existingDisbursement.EndDate = updatedDisbursement.EndDate;
                        existingDisbursement.Title = updatedDisbursement.Title;
                        existingDisbursement.LastUpdatedBy = leader.FullName;
                        existingDisbursement.DisbursementStatus = DisbursementStatusEnum.PENDING;
                        _disbursementRepository.Update(existingDisbursement);
                        disbursementList.Add(existingDisbursement); // Add to list for further processing
                    }
                    else
                    {
                        // If no matching disbursement exists, add a new one
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
                            OrderCode = GenerateUniqueBookingCode(),
                            IsValidWithContract = false
                        };
                        _disbursementRepository.Add(newDisbursement);
                        disbursementList.Add(newDisbursement); // Add to list for further processing
                    }
                }

                // Upload updated contract to SignNow
                await _signNowService.AuthenticateAsync();
                contract.SignNowDocumentId = await _signNowService.UploadInvestmentContractToSignNowAsync(
                    contract,
                    investor,
                    leader,
                    userInProject.Project,
                    shareEquity,
                    disbursementList,
                    investmentContractUpdateDTO.InvestorInfo.BuyPrice
                );

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return contract;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the contract.");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }


    }
}
