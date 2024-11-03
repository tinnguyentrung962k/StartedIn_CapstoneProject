using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.DTOs.RequestDTO;
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
using Microsoft.AspNetCore.Identity;
using StartedIn.CrossCutting.Constants;

namespace StartedIn.Service.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<TaskEntity> _logger;
        private readonly ITaskHistoryRepository _taskHistoryRepository;
        private readonly UserManager<User> _userManager;
        private readonly IProjectRepository _projectRepository;
        private readonly IMilestoneRepository _milestoneRepository;
        private readonly IUserService _userService;

        public TaskService(
            IUnitOfWork unitOfWork,
            ITaskRepository taskRepository,
            ILogger<TaskEntity> logger,
            ITaskHistoryRepository taskHistoryRepository,
            UserManager<User> userManager,
            IProjectRepository projectRepository,
            IMilestoneRepository milestoneRepository,
            IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _taskRepository = taskRepository;
            _logger = logger;
            _taskHistoryRepository = taskHistoryRepository;
            _userManager = userManager;
            _projectRepository = projectRepository;
            _milestoneRepository = milestoneRepository;
            _userService = userService;
        }
        public async Task<TaskEntity> CreateTask(TaskCreateDTO taskCreateDto, string userId)
        {
            var mileStone = await _milestoneRepository.QueryHelper()
                .Include(x=>x.Project)
                .Filter(x=>x.Id.Equals(taskCreateDto.MilestoneId))
                .GetOneAsync();
            if (mileStone is null) {
                throw new NotFoundException(MessageConstant.NotFoundMilestoneError);
            }
            var logInUser = await _userService.CheckIfUserInProject(userId, mileStone.Project.Id);
            try
            {
                _unitOfWork.BeginTransaction();
                TaskEntity task = new TaskEntity
                {
                    MilestoneId = taskCreateDto.MilestoneId,
                    Title = taskCreateDto.TaskTitle,
                    Description = taskCreateDto.Description,
                    Deadline = taskCreateDto.Deadline,
                    Status = TaskStatusConstant.Pending
                };
                var taskEntity = _taskRepository.Add(task);
                string notification = logInUser.User.FullName + " đã tạo ra công việc: " + task.Title;
                TaskHistory history = new TaskHistory
                {
                    Content = notification,
                    CreatedBy = logInUser.User.FullName,
                    TaskId = task.Id
                };
                var taskHistory = _taskHistoryRepository.Add(history);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return taskEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo công việc");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public string GetTaskStatusName(TaskEntityStatus taskStatus)
        {
            return taskStatus switch
            {
                TaskEntityStatus.Pending => TaskStatusConstant.Pending,
                TaskEntityStatus.InProgress => TaskStatusConstant.InProgress,
                TaskEntityStatus.Done => TaskStatusConstant.Done,
                _ => throw new ArgumentOutOfRangeException(nameof(taskStatus), $"Giai đoạn không hợp lệ: {taskStatus}")
            };
        }

        public async Task<TaskEntity> UpdateTaskInfo(string userId, string id, UpdateTaskInfoDTO updateTaskInfoDTO)
        {
            var chosenTask = await _taskRepository.QueryHelper()
                .Include(x=>x.Milestone)
                .Filter(x=>x.Id.Equals(id)).
                GetOneAsync();
            if (chosenTask == null)
            {
                throw new NotFoundException("Không tìm thấy công việc");
            }
            var userInProject = await _userService.CheckIfUserInProject(userId,chosenTask.Milestone.ProjectId);
            try
            {
                string taskStatus = GetTaskStatusName(updateTaskInfoDTO.Status);
                chosenTask.Title = updateTaskInfoDTO.TaskTitle;
                chosenTask.Description = updateTaskInfoDTO.Description;
                chosenTask.Status = taskStatus;
                chosenTask.Deadline = updateTaskInfoDTO.Deadline;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                _taskRepository.Update(chosenTask);
                await _unitOfWork.SaveChangesAsync();
                return chosenTask;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Failed while update task");
            }
        }
    }
}
