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
    public class TerminationConfirmationService : ITerminationConfirmationService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITerminationRequestRepository _terminationRequestRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IContractRepository _contractRepository;
        private readonly UserManager<User> _userManager;
        private ILogger<TerminationRequestService> _logger;
        private readonly ITerminationConfirmRepository _terminationConfirmRepository;
        private readonly IMapper _mapper;

        public TerminationConfirmationService(
            IProjectRepository projectRepository,
            ITerminationRequestRepository terminationRequestRepository,
            IUnitOfWork unitOfWork,
            IUserService userService,
            IContractRepository contractRepository,
            UserManager<User> userManager,
            ILogger<TerminationRequestService> logger,
            ITerminationConfirmRepository terminationConfirmRepository,
            IMapper mapper)
        {
            _projectRepository = projectRepository;
            _terminationRequestRepository = terminationRequestRepository;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _contractRepository = contractRepository;
            _userManager = userManager;
            _logger = logger;
            _terminationConfirmRepository = terminationConfirmRepository;
            _mapper = mapper;
        }

        public async Task<List<TerminationConfirmationResponseDTO>> GetTerminationConfirmationForUserInProject(string userId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var confirmList = await _terminationConfirmRepository.GetTerminationConfirmationForUserInProject(userId, projectId);
            var response = _mapper.Map<List<TerminationConfirmationResponseDTO>>(confirmList);
            return response;
        }

        public async Task AcceptTerminationRequest(string userId, string projectId, string confirmId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var confirmation = await _terminationConfirmRepository.QueryHelper()
                .Include(x => x.TerminationRequest)
                .Filter(x=>x.Id == confirmId)
                .GetOneAsync();
                  
            if (confirmation == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTerminateRequest);
            }
            if (confirmation.TerminationRequest.FromId == userId || confirmation.TerminationRequest.Status != CrossCutting.Enum.TerminationStatus.WAITING || confirmation.ConfirmUserId != userId)
            {
                throw new InvalidDataException(MessageConstant.YouCannotAcceptOrRejectTermination);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                if (confirmation != null)
                {
                    confirmation.IsAgreed = true;
                    confirmation.LastUpdatedBy = userInProject.User.FullName;
                    confirmation.LastUpdatedTime = DateTimeOffset.UtcNow;
                    _terminationConfirmRepository.Update(confirmation);
                }
                var allConfirmationsAgreed = await _terminationConfirmRepository.QueryHelper()
                .Filter(x => x.TerminationRequestId.Equals(confirmation.TerminationRequestId) && x.IsAgreed == false)
                .GetAllAsync();

                var request = await _terminationRequestRepository.GetOneAsync(confirmation.TerminationRequestId);

                if (!allConfirmationsAgreed.Any())
                {
                    request.Status = CrossCutting.Enum.TerminationStatus.ACCEPTED;
                    request.LastUpdatedBy = userInProject.User.FullName;
                    request.LastUpdatedTime = DateTimeOffset.UtcNow;
                    _terminationRequestRepository.Update(request);
                }

                // Commit transaction
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while accept request");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task RejectTerminationRequest(string userId, string projectId, string confirmId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var confirmation = await _terminationConfirmRepository.QueryHelper()
                .Include(x => x.TerminationRequest)
                .Filter(x => x.Id == confirmId)
                .GetOneAsync();

            if (confirmation == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTerminateRequest);
            }
            if (confirmation.TerminationRequest.FromId == userId || confirmation.TerminationRequest.Status != CrossCutting.Enum.TerminationStatus.WAITING || confirmation.ConfirmUserId != userId)
            {
                throw new InvalidDataException(MessageConstant.YouCannotAcceptOrRejectTermination);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                if (confirmation != null)
                {
                    confirmation.IsAgreed = false;
                    confirmation.LastUpdatedBy = userInProject.User.FullName;
                    confirmation.LastUpdatedTime = DateTimeOffset.UtcNow;
                    _terminationConfirmRepository.Update(confirmation);

                    var request = await _terminationRequestRepository.GetOneAsync(confirmation.TerminationRequestId);
                    request.Status = CrossCutting.Enum.TerminationStatus.REJECT;
                    request.LastUpdatedBy = userInProject.User.FullName;
                    request.LastUpdatedTime = DateTimeOffset.UtcNow;
                    _terminationRequestRepository.Update(request);
                }

                // Commit transaction
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while accept request");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<TerminationRequestDetailDTO> GetContractTerminationDetailByConfirmId(string userId, string projectId, string confirmId)
        {
            // Validate if the user is part of the project
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var confirm = await _terminationConfirmRepository.GetOneAsync(confirmId);
            if (confirm == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTerminateRequest);
            }
            // Fetch the termination request by ID
            var terminationRequest = await _terminationRequestRepository.QueryHelper()
                .Include(x => x.Contract)
                .Include(x => x.TerminationConfirmations)
                .Filter(x => x.Id == confirm.TerminationRequestId)
                .GetOneAsync();

            if (terminationRequest == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTerminateRequest);
            }

            if (!terminationRequest.TerminationConfirmations.Any(c => c.ConfirmUserId == userId))
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
