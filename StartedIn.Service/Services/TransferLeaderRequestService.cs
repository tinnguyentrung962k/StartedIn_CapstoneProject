using AutoMapper;
using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TransferLeaderRequest;
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
        private readonly IProjectRepository _projectRepository;
        private readonly IEmailService _emailService;
        private readonly IContractRepository _contractRepository;
        private readonly IMapper _mapper;

        public TransferLeaderRequestService(
            IUnitOfWork unitOfWork, 
            IUserService userService, 
            ITransferLeaderRequestRepository transferLeaderRequestRepository,
            IAppointmentRepository appointmentRepository,
            ILogger<TransferLeaderRequest> logger,
            UserManager<User> userManager,
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            IEmailService emailService,
            IContractRepository contractRepository,
            IMapper mapper
            )
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _transferLeaderRequestRepository = transferLeaderRequestRepository;
            _appointmentRepository = appointmentRepository;
            _logger = logger;
            _userManager = userManager;
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _emailService = emailService;
            _contractRepository = contractRepository;
            _mapper = mapper;
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
                var project = await _projectRepository.GetProjectAndMemberByProjectId(projectId);

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

                foreach (var activeUserInProject in project.UserProjects.Where(x => x.RoleInTeam == CrossCutting.Enum.RoleInTeam.Member || x.RoleInTeam == CrossCutting.Enum.RoleInTeam.Mentor))
                {
                    var user = await _userManager.FindByIdAsync(activeUserInProject.UserId);
                    await _emailService.SendAppointmentInvite(user.Email, project.ProjectName, user.FullName, transferAppointment.MeetingLink, transferAppointment.AppointmentTime.AddHours(7));
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex) {
                _logger.LogError(ex,"Error while sending request");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<TransferLeaderRequest> GetPendingTransferLeaderRequest(string userId, string projectId) 
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var pendingTransferRequest = await _transferLeaderRequestRepository.GetLeaderTransferRequestPending(projectId);
            if (pendingTransferRequest == null)
            {
                throw new NotFoundException(MessageConstant.NoTransferRequestWasFound);
            }
            return pendingTransferRequest;
        }

        public async Task TransferLeaderAfterMeeting(string userId, string projectId, string requestId, LeaderTransferRequestDTO leaderTransferRequestDTO)
        {
            // Verify the current user's role in the project
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }

            // Check if there is a pending leader transfer request for the project
            var transferRequest = await _transferLeaderRequestRepository.GetLeaderTransferRequestPending(projectId);
            if (transferRequest == null)
            {
                throw new NotFoundException(MessageConstant.NoTransferRequestWasFound);
            }

            // Ensure the associated meeting is finished
            if (transferRequest.Appointment.Status != CrossCutting.Enum.MeetingStatus.Finished)
            {
                throw new NotFoundException(MessageConstant.MeetingIsNotFinished);
            }

            // Ensure meeting notes exist for the meeting
            if (transferRequest.Appointment.MeetingNotes == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundMeetingNote);
            }

            // Verify the new leader's role in the project
            var newAssignedLeaderInProject = await _userService.CheckIfUserInProject(leaderTransferRequestDTO.NewLeaderId, projectId);
            if (newAssignedLeaderInProject.RoleInTeam != CrossCutting.Enum.RoleInTeam.Member)
            {
                throw new InvalidAssignRoleException(MessageConstant.CannotTransferLeaderRoleForNotMember);
            }

            try
            {
                // Begin database transaction
                _unitOfWork.BeginTransaction();

                // Update transfer request details
                transferRequest.NewLeader = newAssignedLeaderInProject.User;
                transferRequest.NewLeaderId = newAssignedLeaderInProject.UserId;
                transferRequest.LastUpdatedBy = userInProject.User.FullName;
                transferRequest.LastUpdatedTime = DateTimeOffset.UtcNow;
                transferRequest.TransferDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddHours(7).Date);
                transferRequest.IsAgreed = true;

                _transferLeaderRequestRepository.Update(transferRequest);

                // Update roles in the project
                newAssignedLeaderInProject.RoleInTeam = CrossCutting.Enum.RoleInTeam.Leader;
                await _userRepository.UpdateUserInProject(newAssignedLeaderInProject);

                userInProject.RoleInTeam = CrossCutting.Enum.RoleInTeam.Member;
                await _userRepository.UpdateUserInProject(userInProject);

                // Update contracts for the project
                var contractList = await _contractRepository.GetContractByProjectId(projectId);
                foreach (var contract in contractList)
                {
                    foreach (var userInContract in contract.UserContracts
                        .Where(x => x.Role == CrossCutting.Enum.RoleInContract.CREATOR))
                    {
                        // Handle TransferToId for contract roles
                        if (userInContract.TransferToId != null)
                        {
                            if (userInContract.UserId.Equals(newAssignedLeaderInProject.UserId))
                            {
                                userInContract.TransferToId = null;
                            }
                            else
                            {
                                userInContract.TransferToId = newAssignedLeaderInProject.UserId;
                            }
                        }
                        else
                        {
                            userInContract.TransferToId = newAssignedLeaderInProject.UserId;
                        }

                        await _userRepository.UpdateUserInContract(userInContract);
                    }
                }

                // Commit transaction
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                // Log error and rollback transaction
                _logger.LogError(ex, MessageConstant.UpdateFailed);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task CancelTransferLeaderRequest(string userId, string projectId, string requestId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }

            var transferRequest = await _transferLeaderRequestRepository.GetLeaderTransferRequestPending(projectId);
            if (transferRequest == null)
            {
                throw new NotFoundException(MessageConstant.NoTransferRequestWasFound);
            }
            if (transferRequest.Appointment.Status != CrossCutting.Enum.MeetingStatus.Finished)
            {
                throw new NotFoundException(MessageConstant.MeetingIsNotFinished);
            }
            if (transferRequest.Appointment.MeetingNotes == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundMeetingNote);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                transferRequest.LastUpdatedBy = userInProject.User.FullName;
                transferRequest.LastUpdatedTime = DateTimeOffset.UtcNow;
                transferRequest.IsAgreed = false;

                _transferLeaderRequestRepository.Update(transferRequest);

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

        public async Task<PaginationDTO<TransferLeaderHistoryResponseDTO>> GetLeaderHistoryInTheProject(string userId, string projectId, int page, int size)
        {
            var currentUser = await _userService.CheckIfUserInProject(userId, projectId);
            var historyQuery = _transferLeaderRequestRepository.GetHistoryOfLeaderInAProject(projectId);
            var pagedResult = await historyQuery
                .Skip((page - 1) * size)
                .Take(size).ToListAsync();
            var pagedHistory =  _mapper.Map<List<TransferLeaderHistoryResponseDTO>>(pagedResult);
            var pagedResponse = new PaginationDTO<TransferLeaderHistoryResponseDTO>
            {
                Data = pagedHistory,
                Page = page,
                Size = size,
                Total = historyQuery.ToList().Count()
            };
            return pagedResponse;
        }

        public async Task<TransferLeaderHistoryResponseDTO> GetLeaderHistoryInTheProjectById(string userId, string projectId, string transferId)
        {
            var currentUser = await _userService.CheckIfUserInProject(userId, projectId);
            var transfer = await _transferLeaderRequestRepository.GetLeaderTransferInProjectById(transferId);
            if (transfer == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundLeaderTransfer);
            }
            if (transfer.ProjectId != projectId)
            {
                throw new NotFoundException(MessageConstant.NotFoundLeaderTransfer);
            }
            var response = _mapper.Map<TransferLeaderHistoryResponseDTO>(transfer);
            return response;
        }
    }
}
