﻿using AutoMapper;
using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.LeavingRequest;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.LeavingRequest;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services
{
    public class LeavingRequestService : ILeavingRequestService
    {
        private readonly ILeavingRequestRepository _leavingRequestRepository;
        private readonly UserManager<User> _userManager;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectService _projectService;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LeavingRequestService> _logger; 
        private readonly IAzureBlobService _azureBlobService;
        private readonly IContractRepository _contractRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public LeavingRequestService(
            ILeavingRequestRepository leavingRequestRepository,
            IProjectRepository projectRepository, 
            IProjectService projectService,
            UserManager<User> userManager,
            IUserService userService,
            IUnitOfWork unitOfWork,
            ILogger<LeavingRequestService> logger,
            IAzureBlobService azureBlobService,
            IContractRepository contractRepository,
            IUserRepository userRepository,
            IMapper mapper
            )
        {
            _projectRepository = projectRepository;
            _projectService = projectService;
            _userManager = userManager;
            _leavingRequestRepository = leavingRequestRepository;
            _userService = userService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _azureBlobService = azureBlobService;
            _contractRepository = contractRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }
        public async Task<LeavingRequest> GetLeavingRequestById(string userId, string projectId, string requestId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var request = await _leavingRequestRepository.QueryHelper()
                .Filter(x=>x.ProjectId.Equals(projectId) && x.Id.Equals(requestId))
                .Include(x=>x.User)
                .Include(x=>x.Project)
                .GetOneAsync();
            if (request == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundLeavingRequest);
            }
            return request;
        }

        public async Task<LeavingRequest> CreateLeavingRequest(string userId, string projectId, LeavingRequestCreateDTO leavingRequestCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var existedLeavingRequest = await _leavingRequestRepository.QueryHelper()
                .Filter(x => x.ProjectId.Equals(projectId) 
                && x.UserId.Equals(userId) 
                && x.Status == LeavingRequestStatus.PENDING)
                .GetOneAsync();
            if (existedLeavingRequest != null)
            {
                throw new ExistedRecordException(MessageConstant.PendingLeavingRequestExisted);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var request = new LeavingRequest
                {
                    UserId = userId,
                    ProjectId = projectId,
                    CreatedBy = userInProject.User.FullName,
                    Reason = leavingRequestCreateDTO.Reason,
                    Status = LeavingRequestStatus.PENDING
                };
                var requestEntity = _leavingRequestRepository.Add(request);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return requestEntity;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error while creating request.");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task AcceptLeavingRequest(string userId, string projectId, string requestId, IFormFile confirmFile)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var chosenRequest = await _leavingRequestRepository.QueryHelper()
                .Filter(x=>x.Id.Equals(requestId) 
                && x.ProjectId.Equals(projectId))
                .Include(x => x.User)
                .Include(x => x.Project)
                .GetOneAsync();
            string confirmUrl = await _azureBlobService.UploadEvidenceOfConfirmation(confirmFile);
            if (confirmUrl == null)
            {
                throw new InvalidInputException("Vui lòng nhập file.");
            }
            var project = await _projectRepository.GetProjectById(projectId);
            var activeContractOfUser = await _contractRepository.QueryHelper()
            .Include(c=>c.UserContracts)
            .Filter(c => c.ProjectId.Equals(projectId) 
            && (c.ContractStatus == ContractStatusEnum.SENT || c.ContractStatus == ContractStatusEnum.COMPLETED)
            && c.UserContracts.Any(u => u.UserId.Equals(chosenRequest.UserId)))
            .GetAllAsync();

            if (activeContractOfUser.Any())
            {
                throw new ExistedRecordException(MessageConstant.UserBelongToActiveContracts);
            }
            try 
            {
                _unitOfWork.BeginTransaction();
                chosenRequest.Status = LeavingRequestStatus.ACCEPTED;
                chosenRequest.LastUpdatedBy = userInProject.User.FullName;
                chosenRequest.ConfirmUrl = confirmUrl;
                chosenRequest.LastUpdatedTime = DateTimeOffset.UtcNow;
                var chosenRequestEntity = _leavingRequestRepository.Update(chosenRequest);
                await _userRepository.DeleteUserFromAProject(chosenRequest.UserId, chosenRequest.ProjectId);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while update request.");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task RejectLeavingRequest(string userId, string projectId, string requestId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var chosenRequest = await _leavingRequestRepository.QueryHelper()
                .Filter(x => x.Id.Equals(requestId)
                && x.ProjectId.Equals(projectId))
                .Include(x => x.User)
                .Include(x => x.Project)
                .GetOneAsync();
            try
            {
                _unitOfWork.BeginTransaction();
                chosenRequest.Status = LeavingRequestStatus.REJECTED;
                chosenRequest.LastUpdatedBy = userInProject.User.FullName;
                chosenRequest.LastUpdatedTime = DateTimeOffset.UtcNow;
                var chosenRequestEntity = _leavingRequestRepository.Update(chosenRequest);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while update request.");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<PaginationDTO<LeavingRequestResponseDTO>> FilterLeavingRequestForLeader(string userId, string projectId, LeavingRequestFilterDTO leavingRequestFilterDTO, int page, int size)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var requestQuery = _leavingRequestRepository.GetLeavingRequestForLeaderInProject(projectId);
            if (!string.IsNullOrWhiteSpace(leavingRequestFilterDTO.FullName))
            {
                requestQuery = requestQuery.Where(x => x.User.FullName.ToLower().Contains(leavingRequestFilterDTO.FullName.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(leavingRequestFilterDTO.Email))
            {
                requestQuery = requestQuery.Where(x => x.User.Email.ToLower().Contains(leavingRequestFilterDTO.Email.ToLower()));
            }
            if (leavingRequestFilterDTO.Status != null)
            {
                requestQuery = requestQuery.Where(x => x.Status.Equals(leavingRequestFilterDTO.Status));
            }
            int totalCount = await requestQuery.CountAsync();
            var pagedResult = await requestQuery
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
            var response = _mapper.Map<List<LeavingRequestResponseDTO>>(pagedResult);
            var pagination = new PaginationDTO<LeavingRequestResponseDTO>
            {
                Total = totalCount,
                Page = page,
                Size = size,
                Data = response
            };
            return pagination;
        }

    }
}
