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
        private readonly ITaskboardRepository _taskboardRepository;
        private readonly ITaskHistoryRepository _taskHistoryRepository;
        private readonly UserManager<User> _userManager;

        public TaskService(
            IUnitOfWork unitOfWork,
            ITaskRepository taskRepository,
            ILogger<TaskEntity> logger,
            ITaskboardRepository taskboardRepository,
            ITaskHistoryRepository taskHistoryRepository,
            UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _taskRepository = taskRepository;
            _logger = logger;
            _taskboardRepository = taskboardRepository;
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
                    Position = taskCreateDto.Position,
                    TaskboardId = taskCreateDto.TaskboardId,
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

        public async Task<TaskEntity> MoveTask(string taskId, string taskBoardId, int position, bool needsReposition)
        {
            var chosenTask = await _taskRepository.GetOneAsync(taskId);
            if (chosenTask == null)
            {
                throw new NotFoundException("Không có công việc nào được tìm thấy");
            }

            var chosenTaskBoard = await _taskboardRepository.QueryHelper()
                .Filter(p => p.Id.Equals(taskBoardId))
                .Include(p => p.TasksList)
                .GetOneAsync();
            if (chosenTaskBoard == null)
            {
                throw new NotFoundException("Không có bảng công việc được tìm thấy");
            }

            var oldTaskBoard = await _taskboardRepository.GetOneAsync(chosenTask.TaskboardId);
            if (oldTaskBoard == null)
            {
                throw new NotFoundException("Không có bảng công việc cũ được tìm thấy");
            }
            if (oldTaskBoard.MilestoneId != chosenTaskBoard.MilestoneId)
            {
                throw new Exception("Bảng công việc cũ không cùng cột mốc với bảng công việc mới");
            }

            try
            {
                // Update the chosen major task's new phase information
                chosenTask.Position = position;
                chosenTask.TaskboardId = taskBoardId;
                _taskRepository.Update(chosenTask);

                // Add the task to the new phase's task list
                chosenTaskBoard.TasksList.Add(chosenTask);
                _taskboardRepository.Update(chosenTaskBoard);

                await _unitOfWork.SaveChangesAsync();

                if (needsReposition)
                {
                    _unitOfWork.BeginTransaction();

                    // Reposition tasks in the new phase
                    var newTaskBoardTasks = await _taskRepository.QueryHelper()
                        .Filter(p => p.TaskboardId.Equals(chosenTaskBoard.Id))
                        .OrderBy(p => p.OrderBy(p => p.Position))
                        .GetAllAsync();

                    int newTaskBoardIncrement = (int)Math.Pow(2, 16);
                    int newTaskBoardCurrentPosition = (int)Math.Pow(2, 16);

                    foreach (var task in newTaskBoardTasks)
                    {
                        task.Position = newTaskBoardCurrentPosition;
                        _taskRepository.Update(task);
                        newTaskBoardCurrentPosition += newTaskBoardIncrement;
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                }

                return chosenTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while moving minor task");
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
