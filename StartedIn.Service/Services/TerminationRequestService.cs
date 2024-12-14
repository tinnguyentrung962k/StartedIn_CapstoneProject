using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.TerminationRequest;
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
        private readonly IMapper _mapper;
        private readonly IProjectRepository _projectRepository;

        public TerminationRequestService(
            ITerminationRequestRepository terminationRequestRepository,
            IUnitOfWork unitOfWork,
            IUserService userService,
            IContractRepository contractRepository,
            UserManager<User> userManager,
            ILogger<TerminationRequestService> logger,
            IMapper mapper,
            IProjectRepository projectRepository)
        {
            _terminationRequestRepository = terminationRequestRepository;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _contractRepository = contractRepository;
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
            _projectRepository = projectRepository;
        }
        public async Task CreateTerminationRequest(string userId, string projectId, TerminationRequestCreateDTO requestCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var userInContract = await _userService.CheckIfUserBelongToContract(userId, requestCreateDTO.ContractId);
            var existingRequest = await _terminationRequestRepository.QueryHelper()
                .Filter(x => x.ContractId == requestCreateDTO.ContractId
                && x.FromId == userId
                && x.IsAgreed == null)
                .GetOneAsync();
            var otherExistingRequest = await _terminationRequestRepository.QueryHelper()
                .Filter(x => x.ContractId == requestCreateDTO.ContractId
                && x.IsAgreed == null)
                .GetOneAsync();
            var contract = userInContract.Contract;
            if (contract.ContractType == CrossCutting.Enum.ContractTypeEnum.LIQUIDATIONNOTE)
            {
                throw new InvalidDataException(MessageConstant.YouCannotRequestToTerminateThisContract);
            }
            if (userInContract.Contract.ContractStatus != CrossCutting.Enum.ContractStatusEnum.COMPLETED || 
                existingRequest != null || 
                otherExistingRequest != null || 
                userInProject.RoleInTeam == CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new InvalidDataException(MessageConstant.YouCannotRequestToTerminateThisContract);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var contract = await _contractRepository.GetContractById(requestCreateDTO.ContractId);
                var project = await _projectRepository.GetProjectById(projectId);
                var leaderId = project.UserProjects.FirstOrDefault(x => x.RoleInTeam == CrossCutting.Enum.RoleInTeam.Leader).UserId;
                var terminationRequest = new TerminationRequest
                {
                    ContractId = requestCreateDTO.ContractId,
                    Contract = contract,
                    CreatedBy = userInProject.User.FullName,
                    FromId = userInProject.User.Id,
                    Reason = requestCreateDTO.Reason,
                    ToId = leaderId
                };
                _terminationRequestRepository.Add(terminationRequest);
                contract.CurrentTerminationRequestId = terminationRequest.Id;
                _contractRepository.Update(contract);
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
        public async Task<List<TerminationRequestSentResponseDTO>> GetTerminationRequestForFromUserInProject(string userId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var requestList = await _terminationRequestRepository.GetTerminationRequestForFromUserInProject(userId, projectId);
            var response = _mapper.Map<List<TerminationRequestSentResponseDTO>>(requestList);
            return response;
        }

        public async Task<List<TerminationRequestReceivedResponseDTO>> GetTerminationRequestForToUserInProject(string userId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var requestList = await _terminationRequestRepository.GetTerminationRequestForToUserInProject(userId, projectId);
            var response = _mapper.Map<List<TerminationRequestReceivedResponseDTO>>(requestList);
            return response;
        }

        public async Task AcceptTerminationRequest(string userId, string projectId, string requestId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var request = await _terminationRequestRepository.QueryHelper().Filter(x=>x.Id.Equals(requestId) 
            && x.Contract.ProjectId == projectId)
           .Include(x=>x.Contract).GetOneAsync();
            if (request == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTerminateRequest);
            }
            if (request.IsAgreed != null)
            {
                throw new UpdateException(MessageConstant.UpdateFailed);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                request.IsAgreed = true;
                _terminationRequestRepository.Update(request);
                request.Contract.ContractStatus = CrossCutting.Enum.ContractStatusEnum.WAITINGFORLIQUIDATION;
                _contractRepository.Update(request.Contract);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex) {
                _logger.LogError(ex, MessageConstant.UpdateFailed);
                await _unitOfWork.RollbackAsync();
                throw;
            }

        }

        public async Task RejectTerminationRequest(string userId, string projectId, string requestId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var request = await _terminationRequestRepository.QueryHelper().Filter(x => x.Id.Equals(requestId)
            && x.Contract.ProjectId == projectId)
           .Include(x => x.Contract).GetOneAsync();
            if (request == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTerminateRequest);
            }
            if (request.IsAgreed != null)
            {
                throw new UpdateException(MessageConstant.UpdateFailed);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                request.IsAgreed = false;
                _terminationRequestRepository.Update(request);
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
    }
}
