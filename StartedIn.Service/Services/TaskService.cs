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

namespace StartedIn.Service.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<TaskEntity> _logger;
        private readonly ITaskHistoryRepository _taskHistoryRepository;
        private readonly UserManager<User> _userManager;

        public TaskService(
            IUnitOfWork unitOfWork,
            ITaskRepository taskRepository,
            ILogger<TaskEntity> logger,
            ITaskHistoryRepository taskHistoryRepository,
            UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _taskRepository = taskRepository;
            _logger = logger;
            _taskHistoryRepository = taskHistoryRepository;
            _userManager = userManager;
        }
        public async Task<TaskEntity> CreateTask(TaskCreateDTO taskCreateDto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Người dùng không tồn tại");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                TaskEntity task = new TaskEntity
                {
                    MilestoneId = taskCreateDto.MilestoneId,
                    Title = taskCreateDto.TaskTitle,
                    Description = taskCreateDto.Description,
                    Deadline = taskCreateDto.Deadline,
                    Status = TaskEntityStatus.Pending

                };
                var taskEntity = _taskRepository.Add(task);
                string notification = user.FullName + " đã tạo ra công việc: " + task.Title;
                TaskHistory history = new TaskHistory
                {
                    Content = notification,
                    CreatedBy = user.FullName,
                    TaskId = task.Id
                };
                var taskHistory = _taskHistoryRepository.Add(history);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return taskEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating Minor Task");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<TaskEntity> UpdateTaskInfo(string id, UpdateTaskInfoDTO updateTaskInfoDTO)
        {
            var chosenTask = await _taskRepository.GetOneAsync(id);
            if (chosenTask == null)
            {
                throw new NotFoundException("Không tìm thấy công việc");
            }
            try
            {
                chosenTask.Title = updateTaskInfoDTO.TaskTitle;
                chosenTask.Description = updateTaskInfoDTO.Description;
                chosenTask.Status = updateTaskInfoDTO.Status;
                chosenTask.Deadline = updateTaskInfoDTO.Deadline;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
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
