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
            var confirmList = await _terminationConfirmRepository.GetPendingTerminationConfirmationForUserInProject(userId, projectId);
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
                    var contract = await _contractRepository.GetOneAsync(request.ContractId);
                    contract.ContractStatus = CrossCutting.Enum.ContractStatusEnum.WAITINGFORLIQUIDATION;
                    _contractRepository.Update(contract);
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
    }
}
