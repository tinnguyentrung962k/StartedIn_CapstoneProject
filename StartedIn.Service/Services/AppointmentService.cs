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
using System.Threading.Tasks;

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
        public AppointmentService(IAppointmentRepository appointmentRepository, IUserService userService, IUnitOfWork unitOfWork, ILogger<AppointmentService> logger,
            IEmailService emailService, IProjectRepository projectRepository, IAzureBlobService azureBlobService) 
        {
            _appointmentRepository = appointmentRepository;
            _userService = userService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
            _projectRepository = projectRepository;
            _azureBlobService = azureBlobService;
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
        
        public async Task<IEnumerable<Appointment>> GetAppointmentsByProjectId(string userId, string projectId, int page, int size)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var appointments = await _appointmentRepository.QueryHelper()
                .Filter(x=>x.ProjectId.Equals(projectId))
                .OrderBy(x=>x.OrderByDescending(x=>x.AppointmentTime)).Include(a => a.MeetingNotes)
                .Include(x=>x.Milestone)
                .GetPagingAsync(page, size);
            return appointments;
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
            if (userInProject.RoleInTeam != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }

            var project = await _projectRepository.QueryHelper().Filter(p => p.Id.Equals(projectId)).GetOneAsync();
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
                        newAppointmentEntity.MeetingLink, newAppointmentEntity.AppointmentTime);
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
    }
}
