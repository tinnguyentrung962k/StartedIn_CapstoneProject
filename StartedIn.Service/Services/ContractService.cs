using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;

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
        public ContractService(IContractRepository contractRepository, 
            IUnitOfWork unitOfWork, 
            ILogger<Contract> logger,
            UserManager<User> userManager,
            ISignNowService signNowService,
            IEmailService emailService)
        {
            _contractRepository = contractRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _signNowService = signNowService;
            _emailService = emailService;
        }

        public async Task<Contract> CreateAContract(string userId, ContractCreateThreeModelsDTO contractCreateThreeModelsDTO)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng");
            }

            try
            {
                _unitOfWork.BeginTransaction();
                Contract contract = new Contract
                {
                    ContractName = contractCreateThreeModelsDTO.contractCreateDTO.ContractName,
                    ContractPolicy = contractCreateThreeModelsDTO.contractCreateDTO.ContractPolicy,
                    ContractType = contractCreateThreeModelsDTO.contractCreateDTO.ContractType,
                    CreatedBy = user.FullName,
                    ProjectId = contractCreateThreeModelsDTO.contractCreateDTO.ProjectId,
                };

                List<UserContract> usersInContract = new List<UserContract>();
                foreach (var equityShare in contractCreateThreeModelsDTO.equityShareCreateDTOs)
                {
                    UserContract userContract = new UserContract
                    {
                        ContractId = contract.Id,
                        UserId = equityShare.UserId,
                    };
                    usersInContract.Add(userContract);
                }
                contract.UserContracts = usersInContract;
  
                var userPartys = new List<User>();
                foreach (var userInContract in usersInContract)
                {
                    User userParty = await _userManager.FindByIdAsync(userInContract.UserId);
                    userPartys.Add(userParty);
                }
                var userEmails = new List<string>();
                foreach (var userParty in userPartys)
                {
                    userEmails.Add(userParty.Email);
                }
                var contractEntity = _contractRepository.Add(contract);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return contractEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while creating the contract: {ex.Message}");
                _unitOfWork.RollbackAsync(); 
                throw;
            }
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
                var inviteResposne = await _signNowService.CreateFreeFormInvite(chosenContract.SignNowDocumentId, userEmails);
                _contractRepository.Update(chosenContract);
                await _unitOfWork.SaveChangesAsync();
                return chosenContract;
            }

            catch (Exception ex) {
                _logger.LogError($"An error occurred while upload the contract file: {ex.Message}");
                _unitOfWork.RollbackAsync();
                throw;
            }   
        }
    }
}
