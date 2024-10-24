using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Customize;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<Contract> CreateAContract(string userId, ContractCreateDTO contractCreateDTO,List<EditableField> editableFields)
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
                    ContractName = contractCreateDTO.ContractName,
                    ContractPolicy = contractCreateDTO.ContractPolicy,
                    ContractType = contractCreateDTO.ContractType,
                    CreatedBy = user.FullName,
                    ProjectId = contractCreateDTO.ProjectId,
                };

                List<UserContract> userInContract = new List<UserContract>();
                foreach (var id in contractCreateDTO.UserIds)
                {
                    UserContract userContract = new UserContract
                    {
                        ContractId = contract.Id,
                        UserId = id,
                    };
                    userInContract.Add(userContract);
                }
                contract.UserContracts = userInContract;

                // Upload the document and generate a signing link
                contract.AttachmentLink = await _signNowService.UploadDocumentAsync(contractCreateDTO.FormFile);
                string contractLink = await _signNowService.GenerateSigningLinkAsync(contract.AttachmentLink,editableFields);
                var userPartys = new List<User>();
                foreach (var id in contractCreateDTO.UserIds)
                {
                    User userParty = await _userManager.FindByIdAsync(id);
                    userPartys.Add(userParty);
                }
                foreach (var userParty in userPartys)
                {
                    await _emailService.SendingSigningContractRequest(user, contractLink);
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
    }
}
