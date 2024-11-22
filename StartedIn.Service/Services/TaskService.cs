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
using StartedIn.CrossCutting.DTOs.BaseDTO;
using Microsoft.EntityFrameworkCore;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;

namespace StartedIn.Service.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<TaskEntity> _logger;
        private readonly ITaskHistoryRepository _taskHistoryRepository;
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
            _milestoneRepository = milestoneRepository;
            _userService = userService;
            _mapper = mapper;
        }
        public async Task<TaskEntity> GetTaskDetail(string userId, string taskId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

            var chosenTask = await _taskRepository.GetTaskDetails(taskId) ?? throw new NotFoundException(MessageConstant.NotFoundTaskError);
            var subTasks = await _taskRepository.QueryHelper().Filter(t => t.ParentTaskId == taskId).GetAllAsync();

            chosenTask.SubTasks = (ICollection<TaskEntity>)subTasks;

            //TODO: Get all comments, attachments 

            return chosenTask;
        }

        public async Task<TaskEntity> CreateTask(TaskCreateDTO taskCreateDto, string userId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject == null)
            {
                throw new NotFoundException(MessageConstant.UserNotInProjectError);
            }

            TaskEntity task = new TaskEntity
            {
                Title = taskCreateDto.Title,
                Description = taskCreateDto.Description,
                Deadline = taskCreateDto.Deadline,
                Status = TaskEntityStatus.NOT_STARTED,
                ManHour = taskCreateDto.ManHour ?? 0,
                IsLate = false,
                ProjectId = projectId,
                CreatedBy = userInProject.User.FullName,
                CreatedTime = DateTimeOffset.UtcNow
            };

            if (!string.IsNullOrEmpty(taskCreateDto.Milestone))
            {
                task.MilestoneId = taskCreateDto.Milestone;
            }

            if (!string.IsNullOrEmpty(taskCreateDto.ParentTask))
            {
                var parentTask = await _taskRepository.GetOneAsync(taskCreateDto.ParentTask);
                //if parent task is already in a milestone then do not assign task to another milestone
                if (parentTask.MilestoneId != null && !string.IsNullOrEmpty(taskCreateDto.Milestone))
                {
                    throw new AssignParentTaskException(MessageConstant.MilestoneFromParentAndFromChildrenError);
                }

                task.ParentTaskId = taskCreateDto.ParentTask;
            }

            try
            {
                _unitOfWork.BeginTransaction();

                foreach (var assignee in taskCreateDto.Assignees)
                {
                    task.UserTasks.Add(new UserTask
                    {
                        UserId = assignee,
                        TaskId = task.Id
                    });
                }

                var taskEntity = _taskRepository.Add(task);
                string notification = userInProject.User.FullName + " đã tạo ra công việc: " + task.Title;
                TaskHistory history = new TaskHistory
                {
                    Content = notification,
                    CreatedBy = userInProject.User.FullName,
                    TaskId = task.Id
                };
                _taskHistoryRepository.Add(history);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return taskEntity;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception(MessageConstant.CreateFailed);
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
                _unitOfWork.BeginTransaction();
                chosenTask.Title = updateTaskInfoDTO.Title;
                chosenTask.Description = updateTaskInfoDTO.Description;
                chosenTask.Deadline = updateTaskInfoDTO.Deadline;
                chosenTask.ManHour = updateTaskInfoDTO.ManHour ?? 0;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                TaskHistory history = new TaskHistory
                {
                    Content = $"{userInProject.User.FullName} đã cập nhật thông tin task {chosenTask.Id}",
                    CreatedBy = userInProject.User.FullName,
                    TaskId = chosenTask.Id
                };
                _taskHistoryRepository.Add(history);
                _taskRepository.Update(chosenTask);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return chosenTask;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception(MessageConstant.UpdateFailed);
            }
        }

        public async Task<PaginationDTO<TaskResponseDTO>> FilterTask(string userId, string projectId, TaskFilterDTO taskFilterDto, int size, int page)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

            var filterTasks = _taskRepository.GetTaskListInAProjectQuery(projectId);

            if (!string.IsNullOrWhiteSpace(taskFilterDto.Title))
            {
                filterTasks = filterTasks.Where(t => t.Title != null &&
                                                     t.Title.ToLower().Contains(taskFilterDto.Title.ToLower()));
            }

            if (taskFilterDto.Status != null)
            {
                filterTasks = filterTasks.Where(t => t.Status == taskFilterDto.Status.Value);
            }

            if (taskFilterDto.IsLate != null)
            {
                filterTasks = filterTasks.Where(t => t.IsLate == taskFilterDto.IsLate.Value);
            }

            if (!string.IsNullOrWhiteSpace(taskFilterDto.AssigneeId))
            {
                filterTasks = filterTasks.Where(t => t.UserTasks.Any(ut => ut.UserId == taskFilterDto.AssigneeId));
            }

            if (!string.IsNullOrWhiteSpace(taskFilterDto.MilestoneId))
            {
                filterTasks = filterTasks.Where(t => t.MilestoneId == taskFilterDto.MilestoneId);
            }

            int totalCount = await filterTasks.CountAsync();

            var pagedResult = await filterTasks
                .Skip((page - 1) * size)
                .Take(size)
                .Include(d => d.UserTasks)
                .ToListAsync();

            var taskResponseDTOs = pagedResult.Select(task => new TaskResponseDTO
            {
                Id = task.Id,
                CreatedBy = task.CreatedBy,
                CreatedTime = task.CreatedTime,
                Deadline = task.Deadline,
                DeletedTime = task.DeletedTime,
                Description = task.Description,
                IsLate = task.IsLate,
                LastUpdatedBy = task.LastUpdatedBy,
                LastUpdatedTime = task.LastUpdatedTime,
                Status = task.Status,
                Title = task.Title
            }).ToList();

            var pagination = new PaginationDTO<TaskResponseDTO>()
            {
                Data = _mapper.Map<IEnumerable<TaskResponseDTO>>(pagedResult),
                Total = totalCount,
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
                _unitOfWork.BeginTransaction();
                chosenTask.Status = updateTaskStatusDTO.Status;
                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
                TaskHistory history = new TaskHistory
                {
                    Content = userInProject.User.FullName + "đã cập nhật trạng thái task " + chosenTask.Id,
                    CreatedBy = userInProject.User.FullName,
                    TaskId = chosenTask.Id
                };
                _taskHistoryRepository.Add(history);
                _taskRepository.Update(chosenTask);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return chosenTask;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception(MessageConstant.UpdateFailed);
            }
        }

        public async Task<TaskEntity> UpdateTaskAssignment(string userId, string taskId, string projectId, UpdateTaskAssignmentDTO updateTaskAssignmentDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var assigneeInProject = await _userService.CheckIfUserInProject(updateTaskAssignmentDTO.AssigneeId, projectId);

            // If role in team is INVESTOR or ADMIN or MENTOR throw exception
            if (assigneeInProject.RoleInTeam == RoleInTeam.Investor || assigneeInProject.RoleInTeam == RoleInTeam.Mentor)
            {
                throw new InvalidAssignRoleException(MessageConstant.AssigneeRoleError);
            }

            var chosenTask = await _taskRepository.QueryHelper().Include(t => t.UserTasks).Filter(t => t.Id == taskId).GetOneAsync();
            if (chosenTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTaskError);
            }

            try
            {
                _unitOfWork.BeginTransaction();
                chosenTask.UserTasks.Add(new UserTask
                {
                    UserId = updateTaskAssignmentDTO.AssigneeId,
                    TaskId = taskId
                });
                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
                TaskHistory history = new TaskHistory
                {
                    Content = $"{userInProject.User.FullName} đã phân công {assigneeInProject.User.FullName} vào task {chosenTask.Id}",
                    CreatedBy = userInProject.User.FullName,
                    TaskId = chosenTask.Id
                };
                _taskHistoryRepository.Add(history);
                _taskRepository.Update(chosenTask);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return chosenTask;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception(MessageConstant.UpdateFailed);
            }
        }

        public async Task<TaskEntity> UpdateTaskUnassignment(string userId, string taskId,
            string projectId, UpdateTaskAssignmentDTO updateTaskAssignmentDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var assigneeInProject = await _userService.CheckIfUserInProject(updateTaskAssignmentDTO.AssigneeId, projectId);

            // If role in team is INVESTOR or ADMIN or MENTOR throw exception
            if (assigneeInProject.RoleInTeam == RoleInTeam.Investor || assigneeInProject.RoleInTeam == RoleInTeam.Mentor)
            {
                throw new InvalidAssignRoleException(MessageConstant.AssigneeRoleError);
            }

            var chosenTask = await _taskRepository.QueryHelper().Include(t => t.UserTasks).Filter(t => t.Id == taskId).GetOneAsync();
            if (chosenTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTaskError);
            }

            try
            {
                _unitOfWork.BeginTransaction();
                var userTask = chosenTask.UserTasks.FirstOrDefault(ut => ut.UserId == updateTaskAssignmentDTO.AssigneeId && ut.TaskId == taskId);
                if (userTask == null)
                {
                    throw new NotFoundException(MessageConstant.NotFoundAssigneeError);
                }
                chosenTask.UserTasks.Remove(userTask);
                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
                TaskHistory history = new TaskHistory
                {
                    Content = $"{userInProject.User.FullName} đã hủy phân công {assigneeInProject.User.FullName} khỏi task {chosenTask.Id}",
                    CreatedBy = userInProject.User.FullName,
                    TaskId = chosenTask.Id
                };
                _taskHistoryRepository.Add(history);
                _taskRepository.Update(chosenTask);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return chosenTask;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception(MessageConstant.UpdateFailed);
            }
        }

        public async Task<TaskEntity> UpdateTaskMilestone(string userId, string taskId, string projectId, UpdateTaskMilestoneDTO updateTaskMilestoneDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

            var chosenTask = await _taskRepository.QueryHelper().Include(t => t.SubTasks).Include(t => t.ParentTask).Filter(t => t.Id == taskId).GetOneAsync();
            if (chosenTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTaskError);
            }

            Milestone? chosenMilestone = null;

            if (!String.IsNullOrEmpty(updateTaskMilestoneDTO.MilestoneId))
            {
                chosenMilestone = await _milestoneRepository.GetOneAsync(updateTaskMilestoneDTO.MilestoneId);
                if (chosenMilestone == null)
                {
                    throw new NotFoundException(MessageConstant.NotFoundMilestoneError);
                }
            }

            // If parent task is already in a milestone then do not assign task to another milestone
            if (chosenTask.ParentTask != null && chosenTask.ParentTask.MilestoneId != null)
            {
                throw new AssignParentTaskException(MessageConstant.AssignChildTaskToMilestoneError);
            }


            try
            {
                _unitOfWork.BeginTransaction();
                chosenTask.MilestoneId = updateTaskMilestoneDTO.MilestoneId ?? null;

                // Update all sub tasks milestone
                foreach (TaskEntity subTask in chosenTask.SubTasks)
                {
                    subTask.MilestoneId = updateTaskMilestoneDTO.MilestoneId ?? null;
                }

                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;

                if (chosenMilestone != null)
                {
                    TaskHistory history = new TaskHistory
                    {
                        Content = $"{userInProject.User.FullName} đã gán task {chosenTask.Id} vào cột mốc {chosenMilestone.Id}",
                        CreatedBy = userInProject.User.FullName,
                        TaskId = chosenTask.Id
                    };
                    _taskHistoryRepository.Add(history);
                }
                else
                {
                    TaskHistory history = new TaskHistory
                    {
                        Content = $"{userInProject.User.FullName} đã hủy gán task {chosenTask.Id} vào cột mốc",
                        CreatedBy = userInProject.User.FullName,
                        TaskId = chosenTask.Id
                    };
                    _taskHistoryRepository.Add(history);
                }

                _taskRepository.Update(chosenTask);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return chosenTask;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception(MessageConstant.UpdateFailed);
            }
        }

        public async Task<TaskEntity> UpdateParentTask(string userId, string taskId, string projectId, UpdateParentTaskDTO updateParentTaskDTO)
        {
            if (taskId == updateParentTaskDTO.ParentTaskId)
            {
                throw new AssignParentTaskException(MessageConstant.AssignParentTaskToSelfError);
            }

            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

            var chosenTask = await _taskRepository.GetOneAsync(taskId);
            if (chosenTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTaskError);
            }

            // If parent task id is empty, remove parent task
            if (string.IsNullOrEmpty(updateParentTaskDTO.ParentTaskId))
            {
                _unitOfWork.BeginTransaction();
                chosenTask.ParentTaskId = null;
                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
                TaskHistory history = new TaskHistory
                {
                    Content = $"{userInProject.User.FullName} đã hủy gán task {chosenTask.Id} vào task cha",
                    CreatedBy = userInProject.User.FullName,
                    TaskId = chosenTask.Id
                };
                var taskHistory = _taskHistoryRepository.Add(history);
                _taskRepository.Update(chosenTask);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return chosenTask;
            }

            // Only continue when update parent task id exists
            var parentTask = await _taskRepository.GetOneAsync(updateParentTaskDTO.ParentTaskId);
            if (parentTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundParentTaskError);
            }

            // Get List of SubTasks and Check if assign parent task to sub task
            var subTasks = await _taskRepository.QueryHelper().Filter(t => t.ParentTaskId == taskId).GetAllAsync();
            foreach (TaskEntity task in subTasks)
            {
                if (task.Id == updateParentTaskDTO.ParentTaskId)
                {
                    throw new AssignParentTaskException(MessageConstant.AssignParentTaskToSubTaskError);
                }
            }

            try
            {
                _unitOfWork.BeginTransaction();
                chosenTask.ParentTaskId = updateParentTaskDTO.ParentTaskId;
                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
                chosenTask.MilestoneId = parentTask.MilestoneId;
                TaskHistory history = new TaskHistory
                {
                    Content = $"{userInProject.User.FullName} đã gán task {chosenTask.Id} vào task cha {parentTask.Id}",
                    CreatedBy = userInProject.User.FullName,
                    TaskId = chosenTask.Id
                };
                var taskHistory = _taskHistoryRepository.Add(history);
                _taskRepository.Update(chosenTask);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return chosenTask;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception(MessageConstant.UpdateFailed);
            }
        }

        public async Task DeleteTask(string userId, string taskId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

            var chosenTask = await _taskRepository.GetOneAsync(taskId);
            if (chosenTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTaskError);
            }

            try
            {
                await _taskRepository.SoftDeleteById(taskId);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception(MessageConstant.DeleteFailed);
            }
        }
    }
}
