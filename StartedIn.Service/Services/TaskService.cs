using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
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
        public async Task<TaskEntity> GetTaskDetail(string userId, string taskId)
        {
            var chosenTask = await _taskRepository.QueryHelper().Include(t => t.Milestone).Filter(t => t.Id.Equals(taskId)).GetOneAsync();
            if (chosenTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTaskError);
            }
            var userInProject = await _userService.CheckIfUserInProject(userId, chosenTask.Milestone.ProjectId);
            return chosenTask;
        }

        public async Task<TaskEntity> CreateTask(TaskCreateDTO taskCreateDto, string userId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject == null)
            {
                throw new NotFoundException(MessageConstant.UserNotInProjectError);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                TaskEntity task = new TaskEntity
                {
                    Title = taskCreateDto.TaskTitle,
                    Description = taskCreateDto.Description,
                    Deadline = taskCreateDto.Deadline,
                    Status = TaskEntityStatus.NOT_STARTED,
                    IsLate = false
                };
                var taskEntity = _taskRepository.Add(task);
                string notification = userInProject.User.FullName + " đã tạo ra công việc: " + task.Title;
                TaskHistory history = new TaskHistory
                {
                    Content = notification,
                    CreatedBy = userInProject.User.FullName,
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

        public async Task<TaskEntity> UpdateTaskInfo(string userId, string id, UpdateTaskInfoDTO updateTaskInfoDTO)
        {
            var chosenTask = await _taskRepository.QueryHelper()
                .Include(x => x.Milestone)
                .Filter(x => x.Id.Equals(id)).
                GetOneAsync();
            if (chosenTask == null)
            {
                throw new NotFoundException("Không tìm thấy công việc");
            }
            var userInProject = await _userService.CheckIfUserInProject(userId, chosenTask.Milestone.ProjectId);
            try
            {
                chosenTask.Title = updateTaskInfoDTO.TaskTitle;
                chosenTask.Description = updateTaskInfoDTO.Description;
                chosenTask.Status = updateTaskInfoDTO.Status;
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

        public async Task<IEnumerable<TaskEntity>> GetAllTask(string userId, string projectId, int size, int page)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject == null) 
            {
                throw new NotFoundException(MessageConstant.UserNotInProjectError);
            }

            var tasksInProject = await _taskRepository.QueryHelper().Filter(t => t.ProjectId == projectId).GetPagingAsync(page, size);
            return tasksInProject;
        }
    }
}
