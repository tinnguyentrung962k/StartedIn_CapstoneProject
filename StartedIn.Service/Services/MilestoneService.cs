using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Milestone;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone;
using StartedIn.CrossCutting.Enum;
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
    public class MilestoneService : IMilestoneService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMilestoneRepository _milestoneRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<Milestone> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IMilestoneHistoryRepository _milestoneHistoryRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public MilestoneService(
            IUnitOfWork unitOfWork,
            IMilestoneRepository milestoneRepository,
            ILogger<Milestone> logger,
            ITaskRepository taskRepository, UserManager<User> userManager,
            IMilestoneHistoryRepository milestoneHistoryRepository,
            IAppointmentRepository appointmentRepository,
            IProjectRepository projectRepository, IUserService userService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _milestoneRepository = milestoneRepository;
            _logger = logger;
            _taskRepository = taskRepository;
            _appointmentRepository = appointmentRepository;
            _userManager = userManager;
            _milestoneHistoryRepository = milestoneHistoryRepository;
            _projectRepository = projectRepository;
            _userService = userService;
            _mapper = mapper;
        }
        public async Task<Milestone> CreateNewMilestone(string userId, string projectId, MilestoneCreateDTO milestoneCreateDto)
        {
            var loginUser = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                Milestone milestone = new Milestone
                {
                    ProjectId = projectId,
                    PhaseId = milestoneCreateDto.PhaseId ?? null,
                    Title = milestoneCreateDto.Title,
                    Description = milestoneCreateDto.Description,
                    StartDate = DateOnly.FromDateTime(milestoneCreateDto.StartDate),
                    EndDate = DateOnly.FromDateTime(milestoneCreateDto.EndDate),
                };
                var milestoneEntity = _milestoneRepository.Add(milestone);
                string notification = loginUser.User.FullName + " đã tạo ra cột mốc: " + milestone.Title;
                MilestoneHistory history = new MilestoneHistory
                {
                    Content = notification,
                    CreatedBy = loginUser.User.FullName,
                    MilestoneId = milestone.Id
                };
                var milestoneHistoryEntity = _milestoneHistoryRepository.Add(history);

                foreach (var meeting in milestoneCreateDto.meetingList)
                {
                    var appointment = new Appointment
                    {
                        MilestoneId = milestone.Id,
                        ProjectId = projectId,
                        Title = meeting.Title,
                        Description = meeting.Description,
                        AppointmentTime = meeting.AppointmentTime,
                        CreatedBy = loginUser.User.FullName,
                        CreatedTime = DateTimeOffset.UtcNow,
                        MeetingLink = meeting.MeetingLink
                    };
                    _appointmentRepository.Add(appointment);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return milestoneEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating Milestone");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<MilestoneDetailsResponseDTO> GetMilestoneById(string userId, string projectId, string id)
        {
            var loginUser = await _userService.CheckIfUserInProject(userId, projectId);
            var milestone = await _milestoneRepository.GetMilestoneDetailById(id);
            if (milestone == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundMilestoneError);
            }
            if (milestone.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.MilestoneNotBelongToProjectError);
            }

            var milestoneDTO = _mapper.Map<MilestoneDetailsResponseDTO>(milestone);
            milestoneDTO.Progress = CalculateProgress(milestone);
            return milestoneDTO;
        }

        public async Task<Milestone> UpdateMilestoneInfo(string userId, string projectId, string id, MilestoneInfoUpdateDTO updateMilestoneInfoDTO)
        {
            var loginUser = await _userService.CheckIfUserInProject(userId, projectId);

            var chosenMilestone = await _milestoneRepository.QueryHelper()
                .Include(x => x.Project)
                .Filter(x => x.Id.Equals(id))
                .GetOneAsync();

            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);

            if (chosenMilestone == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundMilestoneError);
            }

            if (chosenMilestone.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.MilestoneNotBelongToProjectError);
            }

            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                chosenMilestone.Title = updateMilestoneInfoDTO.Title;
                chosenMilestone.Description = updateMilestoneInfoDTO.Description;
                chosenMilestone.StartDate = updateMilestoneInfoDTO.StartDate;
                chosenMilestone.EndDate = updateMilestoneInfoDTO.EndDate;
                chosenMilestone.LastUpdatedTime = DateTimeOffset.UtcNow;
                chosenMilestone.LastUpdatedBy = loginUser.User.FullName;
                chosenMilestone.PhaseId = updateMilestoneInfoDTO.PhaseId;
                _milestoneRepository.Update(chosenMilestone);
                string notification = loginUser.User.FullName + " đã cập nhật cột mốc: " + chosenMilestone.Title;
                MilestoneHistory history = new MilestoneHistory
                {
                    Content = notification,
                    CreatedBy = loginUser.User.FullName,
                    MilestoneId = chosenMilestone.Id
                };
                _milestoneHistoryRepository.Add(history);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return chosenMilestone;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Failed while update milestone");
            }
        }

        public async Task<PaginationDTO<MilestoneResponseDTO>> FilterMilestone(string userId, string projectId, MilestoneFilterDTO milestoneFilterDTO, int page, int size)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var filterMilestones = _milestoneRepository.GetMilestoneListQuery(projectId);
            if (!string.IsNullOrEmpty(milestoneFilterDTO.Title))
            {
                filterMilestones = filterMilestones.Where(m => m.Title.Contains(milestoneFilterDTO.Title));
            }

            if (!string.IsNullOrWhiteSpace(milestoneFilterDTO.PhaseId))
            {
                filterMilestones = filterMilestones.Where(m => m.PhaseId.Equals(milestoneFilterDTO.PhaseId));
            }

            int totalCount = await filterMilestones.CountAsync();
            var pagedResult = await filterMilestones
                .Skip((page - 1) * size)
                .Take(size)
                .Include(x => x.Phase)
                .ToListAsync();

            var milestonePagination = new PaginationDTO<MilestoneResponseDTO>()
            {
                Data = _mapper.Map<List<MilestoneResponseDTO>>(pagedResult),
                Total = totalCount,
                Page = page,
                Size = size
            };

            milestonePagination.Data.ToList().ForEach(m =>
            {
                m.Progress = CalculateProgress(pagedResult.Where(nm => nm.Id.Equals(m.Id)).First());
            });

            return milestonePagination;
        }

        public async Task DeleteMilestone(string userId, string projectId, string id)
        {
            var loginUser = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            var milestone = await _milestoneRepository.QueryHelper().Filter(m => m.Id.Equals(id)).GetOneAsync();
            if (milestone == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundMilestoneError);
            }
            if (milestone.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.MilestoneNotBelongToProjectError);
            }
            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                milestone.DeletedTime = DateTimeOffset.UtcNow;
                _milestoneRepository.Update(milestone);
                string notification = loginUser.User.FullName + " đã xóa cột mốc: " + milestone.Title;
                MilestoneHistory history = new MilestoneHistory
                {
                    Content = notification,
                    CreatedBy = loginUser.User.FullName,
                    MilestoneId = milestone.Id
                };
                _milestoneHistoryRepository.Add(history);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Failed while delete milestone");
            }
        }

        public int? CalculateProgress(Milestone milestone)
        {
            // Get completed man hour of all tasks in a milestone / total man hour of all tasks in a milestone and if total man hour is 0, return 0
            var totalManHour = milestone.Tasks.Sum(t => t.ManHour);
            if (totalManHour == 0)
            {
                return null;
            }
            var completedManHour = milestone.Tasks.Where(t => t.Status == TaskEntityStatus.DONE).Sum(t => t.ManHour);
            return (int)Math.Round((double)completedManHour / totalManHour * 100);
        }

        public async Task<PaginationDTO<MilestoneHistoryResponseDTO>> GetMilestoneHistory(string userId, string projectId, int page, int size)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var milestoneHistories = await _milestoneHistoryRepository.QueryHelper()
                .Filter(x => x.Milestone.ProjectId.Equals(projectId))
                .OrderBy(x => x.OrderBy(x => x.CreatedTime)).GetPagingAsync(page, size);

            var pagination = new PaginationDTO<MilestoneHistoryResponseDTO>
            {
                Data = _mapper.Map<IEnumerable<MilestoneHistoryResponseDTO>>(milestoneHistories),
                Page = page,
                Size = size,
                Total = await _milestoneHistoryRepository.QueryHelper().Filter(x => x.Milestone.ProjectId.Equals(projectId)).GetTotal()
            };

            return pagination;
        }
    }
}
