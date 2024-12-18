using DocumentFormat.OpenXml.Bibliography;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Appointment;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.Service.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AppointmentService> _logger;
        private readonly IEmailService _emailService;
        private readonly IProjectRepository _projectRepository;
        private readonly IAzureBlobService _azureBlobService;
        private readonly ITransferLeaderRequestRepository _transfersLeaderRequestRepository;
        private readonly IMapper _mapper;
        private static readonly string GoogleMeetPattern = @"^(https:\/\/meet\.google\.com\/[a-zA-Z0-9-]+)$";
        private readonly ITerminationRequestRepository _terminationRequestRepository;
        private readonly IContractRepository _contractRepository;

        public AppointmentService(IAppointmentRepository appointmentRepository, IUserService userService,
            IUnitOfWork unitOfWork, ILogger<AppointmentService> logger,
            IEmailService emailService, IProjectRepository projectRepository, IAzureBlobService azureBlobService,
            IMapper mapper, ITransferLeaderRequestRepository transferLeaderRequestRepository, ITerminationRequestRepository terminationRequestRepository,
            IContractRepository contractRepository) 
        {
            _appointmentRepository = appointmentRepository;
            _userService = userService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
            _projectRepository = projectRepository;
            _azureBlobService = azureBlobService;
            _mapper = mapper;
            _transfersLeaderRequestRepository = transferLeaderRequestRepository;
            _terminationRequestRepository = terminationRequestRepository;
            _contractRepository = contractRepository;

        }
        public async Task<IEnumerable<Appointment>> GetAppointmentsInProject(string userId, string projectId, int year)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var appointments = await _appointmentRepository.QueryHelper()
                .Filter(x=>x.ProjectId.Equals(projectId) && x.AppointmentTime.Year == year)
                .OrderBy(x=>x.OrderByDescending(x=>x.AppointmentTime)).Include(a => a.MeetingNotes)
                .GetAllAsync();
            return appointments;
        }
        
        public async Task<PaginationDTO<AppointmentResponseDTO>> GetAppointmentsByProjectId(string userId, string projectId, AppointmentFilterDTO appointmentFilterDTO ,int page, int size)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var appointments = _appointmentRepository.GetAppointmentsByProjectId(projectId);
            if (!string.IsNullOrEmpty(appointmentFilterDTO.MilestoneId))
            {
                appointments = appointments.Where(a => a.MilestoneId == appointmentFilterDTO.MilestoneId);
            }
            if (!string.IsNullOrEmpty(appointmentFilterDTO.Title))
            {
                appointments = appointments.Where(a => a.Title.Contains(appointmentFilterDTO.Title));
            }
            if (appointmentFilterDTO.FromDate.HasValue)
            {
                var fromDate = appointmentFilterDTO.FromDate.Value;
                appointments = appointments.Where(a => DateOnly.FromDateTime(a.AppointmentTime.UtcDateTime.AddHours(7).Date) >= fromDate);
            }
            if (appointmentFilterDTO.ToDate.HasValue)
            {
                var toDate = appointmentFilterDTO.ToDate.Value;
                appointments = appointments.Where(a => DateOnly.FromDateTime(a.AppointmentTime.UtcDateTime.AddHours(7).Date) <= toDate);
            }
            if (appointmentFilterDTO.MeetingStatus.HasValue)
            {
                appointments = appointments.Where(a => a.Status == appointmentFilterDTO.MeetingStatus.Value);
            }

            if (!appointmentFilterDTO.FromDate.HasValue && !appointmentFilterDTO.ToDate.HasValue && !appointmentFilterDTO.MeetingStatus.HasValue && string.IsNullOrEmpty(appointmentFilterDTO.MilestoneId) && string.IsNullOrEmpty(appointmentFilterDTO.Title))
            {
                var currentMonthStart = new DateOnly(DateOnly.FromDateTime(DateTime.UtcNow).Year, DateOnly.FromDateTime(DateTime.UtcNow).Month, 1);
                var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);

                appointments = appointments.Where(a => DateOnly.FromDateTime(a.AppointmentTime.UtcDateTime.AddHours(7).Date) >= currentMonthStart &&
                                                        DateOnly.FromDateTime(a.AppointmentTime.UtcDateTime.AddHours(7).Date) <= currentMonthEnd);
            }

            if (appointmentFilterDTO.IsDescending.HasValue && appointmentFilterDTO.IsDescending.Value)
            {
                appointments = appointments.OrderByDescending(a => a.AppointmentTime.UtcDateTime);
            }
            else
            {
                appointments = appointments.OrderBy(a => a.AppointmentTime.UtcDateTime);
            }

            int totalCount = await appointments.CountAsync();

            var appointmentPaging = await appointments.Skip((page - 1) * size)
                .Take(size).ToListAsync();
            
            var response = new PaginationDTO<AppointmentResponseDTO>
            {
                Data = _mapper.Map<List<AppointmentResponseDTO>>(appointmentPaging),
                Page = page,
                Size = size,
                Total = totalCount
            };
            return response;
        }

        public async Task UpdateAppointmentStatus(string userId, string projectId, string appointmentId, MeetingStatus status)
        {
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var appointment = await _appointmentRepository.QueryHelper().Filter(a => a.Id.Equals(appointmentId)).GetOneAsync();
            if (appointment == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundAppointment);
            }
            if (status == MeetingStatus.Cancelled)
            {
                var transferRequest = await _transfersLeaderRequestRepository.GetLeaderTransferRequestPending(projectId);
                var acceptedTerminateRequest = await _terminationRequestRepository.QueryHelper()
                    .Filter(x => x.AppointmentId.Equals(appointment.Id) && x.IsAgreed == true)
                    .Include(x=>x.Contract)
                    .GetOneAsync();
                if (acceptedTerminateRequest == null && transferRequest == null)
                {
                    throw new NotFoundException(MessageConstant.NotFoundTerminateRequest);
                }

                if (transferRequest != null)
                {
                    transferRequest.IsAgreed = false;
                    _transfersLeaderRequestRepository.Update(transferRequest);
                }
                if (acceptedTerminateRequest != null)
                {
                    acceptedTerminateRequest.IsAgreed = false;
                    _terminationRequestRepository.Update(acceptedTerminateRequest);
                    acceptedTerminateRequest.Contract.ContractStatus = ContractStatusEnum.COMPLETED;
                    _contractRepository.Update(acceptedTerminateRequest.Contract);
                }
                var contract = await _contractRepository.QueryHelper().Filter(x => x.TerminationMeetingId.Equals(appointment.Id)).GetOneAsync();
                if (contract != null)
                {
                    contract.ContractStatus = ContractStatusEnum.COMPLETED;
                    _contractRepository.Update(contract);
                }
            }
            
            appointment.Status = status;
            _appointmentRepository.Update(appointment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Appointment> GetAppointmentsById(string userId, string projectId, string appointmentId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var appointment = await _appointmentRepository.QueryHelper()
                .Filter(x => x.ProjectId.Equals(projectId) && x.Id.Equals(appointmentId))
                .Include(x=>x.Milestone)
                .Include(x=>x.MeetingNotes)
                .GetOneAsync();
            if (appointment == null)
            {
                throw new NotFoundException(MessageConstant.MeetingNotFound);
            }
            return appointment;
        }
        public async Task<Appointment> CreateAnAppointment(string userId, string projectId, AppointmentCreateDTO appointmentCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            
            var project = await _projectRepository.QueryHelper().Filter(p => p.Id.Equals(projectId)).GetOneAsync();
            if (!IsValidAppointmentLink(appointmentCreateDTO.MeetingLink))
            {
                throw new InvalidLinkException(MessageConstant.InvalidLink);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var newAppointment = new Appointment
                {
                    Title = appointmentCreateDTO.Title,
                    AppointmentTime = appointmentCreateDTO.AppointmentTime,
                    Description = appointmentCreateDTO.Description,
                    CreatedBy = userInProject.User.FullName,
                    ProjectId = projectId,
                    MeetingLink = appointmentCreateDTO.MeetingLink,
                    MilestoneId = appointmentCreateDTO.MilestoneId,
                    Status = CrossCutting.Enum.MeetingStatus.Proposed
                };
                var newAppointmentEntity = _appointmentRepository.Add(newAppointment);
                foreach (var user in project.UserProjects)
                {
                    var receiverName = user.User.FullName;
                    var receiveEmail = user.User.Email;
                    await _emailService.SendAppointmentInvite(receiveEmail, project.ProjectName, receiverName,
                        newAppointmentEntity.MeetingLink, newAppointmentEntity.AppointmentTime.AddHours(7));
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return newAppointmentEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while adding appointment to project {projectId} by user {userId}: {ex}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        
        public static bool IsValidAppointmentLink(string link)
        {
            return Regex.IsMatch(link, GoogleMeetPattern, RegexOptions.IgnoreCase);
        }
    }
}
