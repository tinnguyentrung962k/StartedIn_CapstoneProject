using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.TerminationRequest;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationConfirmation;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationRequest;
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
    public class TerminationRequestService : ITerminationRequestService
    {
        private readonly ITerminationRequestRepository _terminationRequestRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IContractRepository _contractRepository;
        private readonly UserManager<User> _userManager;
        private ILogger<TerminationRequestService> _logger;
        private readonly ITerminationConfirmRepository _terminationConfirmRepository;
        private readonly IMapper _mapper;

        public TerminationRequestService(
            ITerminationRequestRepository terminationRequestRepository,
            IUnitOfWork unitOfWork,
            IUserService userService,
            IContractRepository contractRepository,
            UserManager<User> userManager,
            ILogger<TerminationRequestService> logger,
            ITerminationConfirmRepository terminationConfirmRepository,
            IMapper mapper)
        {
            _terminationRequestRepository = terminationRequestRepository;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _contractRepository = contractRepository;
            _userManager = userManager;
            _logger = logger;
            _terminationConfirmRepository = terminationConfirmRepository;
            _mapper = mapper;
        }
        public async Task CreateTerminationRequest(string userId, string projectId, string contractId, TerminationRequestCreateDTO requestCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var userInContract = await _userService.CheckIfUserBelongToContract(userId, contractId);
            var existingRequest = await _terminationRequestRepository.QueryHelper()
                .Filter(x => x.ContractId == contractId
                && x.FromId == userId
                && x.Status == CrossCutting.Enum.TerminationStatus.WAITING)
                .GetOneAsync();
            if (userInContract.Contract.ContractStatus != CrossCutting.Enum.ContractStatusEnum.COMPLETED || existingRequest != null)
            {
                throw new InvalidDataException(MessageConstant.YouCannotRequestToTerminateThisContract);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var contract = await _contractRepository.GetContractById(contractId);
                var userParties = contract.UserContracts;
                var terminationRequest = new TerminationRequest
                {
                    ContractId = contractId,
                    Contract = contract,
                    CreatedBy = userInProject.User.FullName,
                    FromId = userInProject.User.Id,
                    Reason = requestCreateDTO.Reason,
                    Status = CrossCutting.Enum.TerminationStatus.WAITING,
                };
                var confirmList = new List<TerminationConfirmation>();
                foreach (var userParty in userParties.Where(p => p.UserId != userId))
                {
                    var confirm = new TerminationConfirmation
                    {
                        ConfirmUserId = userParty.UserId,
                        CreatedBy = userInProject.User.FullName,
                        TerminationRequestId = terminationRequest.Id,
                        TerminationRequest = terminationRequest
                    };
                    confirmList.Add(confirm);
                }
                _terminationRequestRepository.Add(terminationRequest);
                await _terminationConfirmRepository.AddRangeAsync(confirmList);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error while creating request");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public async Task<List<TerminationRequestResponseDTO>> GetTerminationRequestForUserInProject(string userId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var requestList = await _terminationRequestRepository.GetTerminationRequestForUserInProject(userId, projectId);
            var response = _mapper.Map<List<TerminationRequestResponseDTO>>(requestList);
            return response;
        }

        public async Task<TerminationRequestDetailDTO> GetContractTerminationDetailById(string userId, string projectId, string requestId)
        {
            // Validate if the user is part of the project
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

            // Fetch the termination request by ID
            var terminationRequest = await _terminationRequestRepository.QueryHelper()
                .Include(x => x.Contract)
                .Include(x => x.TerminationConfirmations)
                .Filter(x => x.Id == requestId)
                .GetOneAsync();

            if (terminationRequest == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTerminateRequest);
            }

            if (terminationRequest.FromId != userId)
            {
                throw new UnauthorizedAccessException(MessageConstant.YouAreNotBelongToThisRequest);
            }

            var fromUser = await _userManager.FindByIdAsync(terminationRequest.FromId);

            var termination = new TerminationRequestDetailDTO
            {
                Id = terminationRequest.Id,
                FromId = terminationRequest.FromId,
                FromName = fromUser.FullName,
                ContractId = terminationRequest.ContractId,
                ContractIdNumber = terminationRequest.Contract.ContractIdNumber,
                Reason = terminationRequest.Reason
            };

            // Prepare the list of user responses
            var listUserPartiesInContract = new List<UserPartyInContractInTerminationResponseDTO>();
            foreach (var confirmation in terminationRequest.TerminationConfirmations)
            {
                var userInContract = await _userManager.FindByIdAsync(confirmation.ConfirmUserId);
                if (userInContract != null)
                {
                    listUserPartiesInContract.Add(new UserPartyInContractInTerminationResponseDTO
                    {
                        ToId = confirmation.ConfirmUserId,
                        ToName = userInContract.FullName,
                        IsAgreed = confirmation.IsAgreed,
                    });
                }
            }

            termination.UserParties = listUserPartiesInContract;
            return termination;
        }
    }
}
