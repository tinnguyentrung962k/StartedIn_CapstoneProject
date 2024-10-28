using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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

        public MilestoneService(
            IUnitOfWork unitOfWork,
            IMilestoneRepository milestoneRepository,
            ILogger<Milestone> logger,
            ITaskRepository taskRepository, UserManager<User> userManager, 
            IMilestoneHistoryRepository milestoneHistoryRepository, 
            IProjectRepository projectRepository)
        {
            _unitOfWork = unitOfWork;
            _milestoneRepository = milestoneRepository;
            _logger = logger;
            _taskRepository = taskRepository;
            _userManager = userManager;
            _milestoneHistoryRepository = milestoneHistoryRepository;
            _projectRepository = projectRepository;
        }
        public async Task<Milestone> CreateNewMilestone(string userId, MilestoneCreateDTO milestoneCreateDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Người dùng không tồn tại");
            }
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, milestoneCreateDto.ProjectId);
            if (projectRole != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException("Bạn không phải nhóm trưởng");
            }
            try
            {  
                string phaseName = GetPhaseName(milestoneCreateDto.PhaseEnum);
                _unitOfWork.BeginTransaction();
                Milestone milestone = new Milestone
                {
                    ProjectId = milestoneCreateDto.ProjectId,
                    Title = milestoneCreateDto.MilstoneTitle,
                    Description = milestoneCreateDto.Description,
                    DueDate = milestoneCreateDto.DueDate,
                    ExtendedCount = 0,
                    PhaseName = phaseName
                };
                var milestoneEntity = _milestoneRepository.Add(milestone);
                string notification = user.FullName + " đã tạo ra cột mốc: " + milestone.Title;
                MilestoneHistory history = new MilestoneHistory
                {
                    Content = notification,
                    CreatedBy = user.FullName,
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

        public string GetPhaseName(PhaseEnum phaseEnum)
        {
            return phaseEnum switch
            {
                PhaseEnum.Initializing => "Giai đoạn khởi động",
                PhaseEnum.Planning => "Giai đoạn lập kế hoạch",
                PhaseEnum.Executing => "Giai đoạn triển khai",
                PhaseEnum.Closing => "Giai đoạn kết thúc",
                _ => throw new ArgumentOutOfRangeException(nameof(phaseEnum), $"Giai đoạn không hợp lệ: {phaseEnum}")
            };
        }

        public async Task<Milestone> GetMilestoneById(string id)
        {
            var milestone = await _milestoneRepository.GetMilestoneDetailById(id);
            if (milestone == null)
            {
                throw new NotFoundException("Không tìm thấy cột mốc");
            }
            return milestone;
        }

        public async Task<Milestone> UpdateMilestoneInfo(string id, MilestoneInfoUpdateDTO updateMilestoneInfoDTO)
        {
            var chosenMilestone = await _milestoneRepository.GetOneAsync(id);
            if (chosenMilestone == null)
            {
                throw new NotFoundException("Không tìm thấy cột mốc");
            }
            try
            {
                chosenMilestone.Title = updateMilestoneInfoDTO.MilestoneTitle;
                chosenMilestone.Description = updateMilestoneInfoDTO.Description;
                chosenMilestone.DueDate = updateMilestoneInfoDTO.DueDate;
                chosenMilestone.LastUpdatedTime = DateTimeOffset.UtcNow;
                _milestoneRepository.Update(chosenMilestone);
                await _unitOfWork.SaveChangesAsync();
                return chosenMilestone;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Failed while update task");
            }
        }
    }
}
