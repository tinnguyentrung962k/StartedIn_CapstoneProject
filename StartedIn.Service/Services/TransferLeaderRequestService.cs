using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
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
    public class TransferLeaderRequestService : ITransferLeaderRequestService
    {
        private readonly IUserService _userService;
        private readonly ITransferLeaderRequestRepository _transferLeaderRequestRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransferLeaderRequest> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;

        public TransferLeaderRequestService(
            IUnitOfWork unitOfWork, 
            IUserService userService, 
            ITransferLeaderRequestRepository transferLeaderRequestRepository,
            IAppointmentRepository appointmentRepository,
            ILogger<TransferLeaderRequest> logger,
            UserManager<User> userManager,
            IUserRepository userRepository
            )
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _transferLeaderRequestRepository = transferLeaderRequestRepository;
            _appointmentRepository = appointmentRepository;
            _logger = logger;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task CreateLeaderTransferRequestInAProject(string userId, string projectId, TerminationMeetingCreateDTO terminationMeetingCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var existingRequest = await _transferLeaderRequestRepository.QueryHelper()
                .Filter(r => r.ProjectId == projectId && r.IsAgreed == null && r.FormerLeaderId == userId)
                .GetOneAsync();

            if (existingRequest != null)
            {
                throw new ExistedRecordException(MessageConstant.ExistingTransferLeaderRequestWasFound);
            }

            try
            {
                _unitOfWork.BeginTransaction();
                Appointment appointment = new Appointment
                {
                    ProjectId = userInProject.ProjectId,
                    AppointmentTime = terminationMeetingCreateDTO.AppointmentTime,
                    CreatedBy = userInProject.User.FullName,
                    Description = terminationMeetingCreateDTO.Description,
                    MeetingLink = terminationMeetingCreateDTO.MeetingLink,
                    Status = CrossCutting.Enum.MeetingStatus.Proposed,
                    Title = terminationMeetingCreateDTO.Title
                };
                var transferAppointment = _appointmentRepository.Add(appointment);
                TransferLeaderRequest transferLeaderRequest = new TransferLeaderRequest
                {
                    CreatedBy = userInProject.User.FullName,
                    FormerLeaderId = userInProject.UserId,
                    FormerLeader = userInProject.User,
                    Project = userInProject.Project,
                    ProjectId = userInProject.ProjectId,
                    Appointment = transferAppointment,
                    AppointmentId = transferAppointment.Id
                };
                var newTransferRequest = _transferLeaderRequestRepository.Add(transferLeaderRequest);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex) {
                _logger.LogError(ex,"Error while sending request");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task TransferLeaderAfterMeeting(string userId, string projectId, string newLeaderId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }

            var transferRequest = await _transferLeaderRequestRepository.QueryHelper()
                .Filter(x => x.FormerLeaderId.Equals(userId)
                && x.ProjectId.Equals(projectId)
                && x.IsAgreed == null)
                .Include(x=>x.Appointment)
                .GetOneAsync();
            if (transferRequest == null) 
            {
                throw new NotFoundException(MessageConstant.NoTransferRequestWasFound);
            }
            if (transferRequest.Appointment.Status != CrossCutting.Enum.MeetingStatus.Finished)
            {
                throw new NotFoundException(MessageConstant.MeetingIsNotFinished);
            }
            var newLeader = await _userManager.FindByIdAsync(userId);
            if (newLeader == null) 
            {
                throw new NotFoundException(MessageConstant.NotFoundUserError);
            }

            try
            {
                _unitOfWork.BeginTransaction();
                transferRequest.NewLeader = newLeader;
                transferRequest.NewLeaderId = newLeaderId;
                transferRequest.LastUpdatedBy = userInProject.User.FullName;
                transferRequest.LastUpdatedTime = DateTimeOffset.UtcNow;
                transferRequest.TransferDate = DateOnly.FromDateTime(DateTime.UtcNow);
                transferRequest.IsAgreed = true;

                _transferLeaderRequestRepository.Update(transferRequest);

                var newLeaderInProject = await _userService.CheckIfUserInProject(newLeaderId, projectId);

                newLeaderInProject.RoleInTeam = CrossCutting.Enum.RoleInTeam.Leader;
                userInProject.RoleInTeam = CrossCutting.Enum.RoleInTeam.Member;

                _userRepository.UpdateUserInProject(newLeaderInProject);
                _userRepository.UpdateUserInProject(userInProject);

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
