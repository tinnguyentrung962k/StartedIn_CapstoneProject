using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using Microsoft.AspNetCore.Identity;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Tasks;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using AutoMapper;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;

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
        private readonly IMapper _mapper;


        public TaskService(
            IUnitOfWork unitOfWork,
            ITaskRepository taskRepository,
            ILogger<TaskEntity> logger,
            ITaskHistoryRepository taskHistoryRepository,
            UserManager<User> userManager,
            IProjectRepository projectRepository,
            IMilestoneRepository milestoneRepository,
            IUserService userService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _taskRepository = taskRepository;
            _logger = logger;
            _taskHistoryRepository = taskHistoryRepository;
            _userManager = userManager;
            _projectRepository = projectRepository;
            _milestoneRepository = milestoneRepository;
            _userService = userService;
            _mapper = mapper;
        }
        public async Task<TaskEntity> GetTaskDetail(string userId, string taskId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

            var chosenTask = await _taskRepository.GetOneAsync(taskId);
            if (chosenTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTaskError);
            }
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
                    Title = taskCreateDto.Title,
                    Description = taskCreateDto.Description,
                    Deadline = taskCreateDto.Deadline,
                    Status = TaskEntityStatus.NOT_STARTED,
                    IsLate = false,
                    ProjectId = projectId,
                    CreatedBy = userInProject.User.FullName,
                    CreatedTime = DateTimeOffset.UtcNow
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

        public async Task<TaskEntity> UpdateTaskInfo(string userId, string taskId,
            string projectId, UpdateTaskInfoDTO updateTaskInfoDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

            var chosenTask = await _taskRepository.GetOneAsync(taskId);
            if (chosenTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTaskError);
            }

            try
            {
                chosenTask.Title = updateTaskInfoDTO.Title;
                chosenTask.Description = updateTaskInfoDTO.Description;
                chosenTask.Deadline = updateTaskInfoDTO.Deadline;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                TaskHistory history = new TaskHistory
                {
                    Content = $"{userInProject.User.FullName} đã cập nhật thông tin task {chosenTask.Id}",
                    CreatedBy = userInProject.User.FullName,
                    TaskId = chosenTask.Id
                };
                var taskHistory = _taskHistoryRepository.Add(history);

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

        public async Task<PaginationDTO<TaskResponseDTO>> GetAllTask(string userId, string projectId, int size, int page)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

            var tasksInProjectQuery = _taskRepository.QueryHelper().Filter(t => t.ProjectId == projectId);

            var pagination = new PaginationDTO<TaskResponseDTO>()
            {
                Data = _mapper.Map<IEnumerable<TaskResponseDTO>>(await tasksInProjectQuery.GetPagingAsync(page, size)),
                Total = await tasksInProjectQuery.GetTotal(),
                Page = page,
                Size = size
            };

            return pagination;
        }

        public async Task<TaskEntity> UpdateTaskStatus(string userId, string taskId, string projectId, UpdateTaskStatusDTO updateTaskStatusDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

            var chosenTask = await _taskRepository.GetOneAsync(taskId);
            if (chosenTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTaskError);
            }

            try
            {
                chosenTask.Status = updateTaskStatusDTO.Status;
                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
                TaskHistory history = new TaskHistory
                {
                    Content = userInProject.User.FullName + "đã cập nhật trạng thái task " + chosenTask.Id,
                    CreatedBy = userInProject.User.FullName,
                    TaskId = chosenTask.Id
                };
                var taskHistory = _taskHistoryRepository.Add(history);
                _taskRepository.Update(chosenTask);
                await _unitOfWork.SaveChangesAsync();
                return chosenTask;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Failed while update task status");
            }
        }

        public async Task<TaskEntity> UpdateTaskAssignment(string userId, string taskId, string projectId, UpdateTaskAssignmentDTO updateTaskAssignmentDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var assigneeInProject = await _userService.CheckIfUserInProject(updateTaskAssignmentDTO.AssigneeId, projectId);

            var chosenTask = await _taskRepository.QueryHelper().Include(t => t.UserTasks).Filter(t => t.Id == taskId).GetOneAsync();
            if (chosenTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTaskError);
            }

            try
            {
                chosenTask.UserTasks.Add(new UserTask
                {
                    UserId = updateTaskAssignmentDTO.AssigneeId,
                    TaskId = taskId
                });
                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
                string message = $"{} abc";
                TaskHistory history = new TaskHistory
                {
                    Content = $"{userInProject.User.FullName} đã phân công {assigneeInProject.User.FullName} vào task {chosenTask.Id}",
                    CreatedBy = userInProject.User.FullName,
                    TaskId = chosenTask.Id
                };
                var taskHistory = _taskHistoryRepository.Add(history);
                _taskRepository.Update(chosenTask);
                await _unitOfWork.SaveChangesAsync();
                return chosenTask;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Failed while update task assignment");
            }
        }

        public async Task<TaskEntity> UpdateTaskMilestone(string userId, string taskId, string projectId, UpdateTaskMilestoneDTO updateTaskMilestoneDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

            var chosenTask = await _taskRepository.GetOneAsync(taskId);
            if (chosenTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTaskError);
            }

            var chosenMilestone = await _milestoneRepository.GetOneAsync(updateTaskMilestoneDTO.MilestoneId);
            if (chosenMilestone == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundMilestoneError);
            }

            try
            {
                chosenTask.MilestoneId = updateTaskMilestoneDTO.MilestoneId;
                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
                TaskHistory history = new TaskHistory
                {
                    Content = $"{userInProject.User.FullName} đã gán task {chosenTask.Id} vào mục tiêu {chosenMilestone.Id}",
                    CreatedBy = userInProject.User.FullName,
                    TaskId = chosenTask.Id
                };
                var taskHistory = _taskHistoryRepository.Add(history);
                _taskRepository.Update(chosenTask);
                await _unitOfWork.SaveChangesAsync();
                return chosenTask;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Failed while update task milestone");
            }
        }
    }
}
