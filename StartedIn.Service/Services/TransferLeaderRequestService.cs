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

        public TransferLeaderRequestService(
            IUnitOfWork unitOfWork, 
            IUserService userService, 
            ITransferLeaderRequestRepository transferLeaderRequestRepository,
            IAppointmentRepository appointmentRepository,
            ILogger<TransferLeaderRequest> logger
            )
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _transferLeaderRequestRepository = transferLeaderRequestRepository;
            _appointmentRepository = appointmentRepository;
            _logger = logger;
        }

        public async Task CreateLeaderTransferRequestInAProject(string userId, string projectId, TerminationMeetingCreateDTO terminationMeetingCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
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
    }
}
