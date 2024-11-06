using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO;
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
        private readonly IProjectRepository _projectRepository;
        private readonly IUserService _userService;

        public MilestoneService(
            IUnitOfWork unitOfWork,
            IMilestoneRepository milestoneRepository,
            ILogger<Milestone> logger,
            ITaskRepository taskRepository, UserManager<User> userManager, 
            IMilestoneHistoryRepository milestoneHistoryRepository, 
            IProjectRepository projectRepository, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _milestoneRepository = milestoneRepository;
            _logger = logger;
            _taskRepository = taskRepository;
            _userManager = userManager;
            _milestoneHistoryRepository = milestoneHistoryRepository;
            _projectRepository = projectRepository;
            _userService = userService;
        }
        public async Task<Milestone> CreateNewMilestone(string userId, string projectId ,MilestoneCreateDTO milestoneCreateDto)
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
                    Title = milestoneCreateDto.MilstoneTitle,
                    Description = milestoneCreateDto.Description,
                    DueDate = milestoneCreateDto.DueDate,
                    ExtendedCount = 0,
                    PhaseName = milestoneCreateDto.PhaseEnum
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

        public async Task<Milestone> GetMilestoneById(string userId, string projectId, string id)
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
            return milestone;
        }

        public async Task<Milestone> UpdateMilestoneInfo(string userId, string projectId, string id, MilestoneInfoUpdateDTO updateMilestoneInfoDTO)
        {
            var loginUser = await _userService.CheckIfUserInProject(userId, projectId);
            
            var chosenMilestone = await _milestoneRepository.QueryHelper()
                .Include(x=>x.Project)
                .Filter(x=>x.Id.Equals(id))
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
                chosenMilestone.Title = updateMilestoneInfoDTO.MilestoneTitle;
                chosenMilestone.Description = updateMilestoneInfoDTO.Description;
                chosenMilestone.DueDate = updateMilestoneInfoDTO.DueDate;
                chosenMilestone.LastUpdatedTime = DateTimeOffset.UtcNow;
                chosenMilestone.LastUpdatedBy = loginUser.User.FullName;
                _milestoneRepository.Update(chosenMilestone);
                await _unitOfWork.SaveChangesAsync();
                return chosenMilestone;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Failed while update milestone");
            }
        }
    }
}
