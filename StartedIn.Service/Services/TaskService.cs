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
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IProjectRepository _projectRepository;

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
            _projectRepository = projectRepository;
        }
        public async Task<TaskDetailDTO> GetTaskDetail(string userId, string taskId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

            var chosenTask = await _taskRepository.GetTaskDetails(taskId) ?? throw new NotFoundException(MessageConstant.NotFoundTaskError);
            // return this if the task is not a parent task
            var response = _mapper.Map<TaskDetailDTO>(chosenTask);

            if (chosenTask.ParentTaskId != null)
            {
                return response;
            }

            var subTasks = await _taskRepository.GetSubTaskDetails(chosenTask.Id);
            chosenTask.SubTasks = subTasks;
            var userTaskResponses = new List<UserTaskResponseDTO>();
            foreach (var subTask in subTasks)
            {
                var userTasks = subTask.UserTasks.Where(ut => ut.TaskId.Equals(subTask.Id)).ToList();
                foreach (var userTask in userTasks)
                {
                    var userTaskResponse = new UserTaskResponseDTO
                    {
                        UserId = userTask.UserId,
                        TaskId = userTask.TaskId,
                        FullName = userTask.User.FullName,
                        TaskName = userTask.Task.Title,
                        ActualManHour = userTask.ActualManHour,
                        LastUpdatedTime = userTask.LastUpdatedTime
                    };
                    userTaskResponses.Add(userTaskResponse);
                    response.ActualManHour += userTaskResponse.ActualManHour;
                }
            }
            if (chosenTask.ParentTaskId == null)
            {
                response.UserTasks = userTaskResponses;
            }
            response.SubTasks = _mapper.Map<ICollection<TaskResponseDTO>>(subTasks);

            return response;
        }

        public async Task<TaskEntity> CreateTask(TaskCreateDTO taskCreateDto, string userId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject == null)
            {
                throw new NotFoundException(MessageConstant.UserNotInProjectError);
            }

            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (taskCreateDto.ParentTask == null && projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.CannotCreateParentTask);
            }

            var existingChildrenTasks = _taskRepository.GetTaskListInAProjectQuery(projectId).Where(t => t.ParentTaskId != null);

            TaskEntity task = new TaskEntity
            {
                Title = taskCreateDto.Title,
                Description = taskCreateDto.Description,
                StartDate = taskCreateDto.StartDate,
                EndDate = taskCreateDto.EndDate,
                Status = TaskEntityStatus.NOT_STARTED,
                ManHour = taskCreateDto.ManHour ?? 0,
                Priority = taskCreateDto.Priority ?? 0,
                IsLate = false,
                ProjectId = projectId,
                CreatedBy = userInProject.User.FullName,
                CreatedTime = DateTimeOffset.UtcNow
            };

            if (taskCreateDto.Assignees.Length > 1)
            {
                throw new InvalidInputException(MessageConstant.NoMoreThanOneAssignee);
            }
            if (!string.IsNullOrEmpty(taskCreateDto.Milestone))
            {
                task.MilestoneId = taskCreateDto.Milestone;
            }

            if (!string.IsNullOrEmpty(taskCreateDto.ParentTask))
            {
                var parentTask = await _taskRepository.GetTaskDetails(taskCreateDto.ParentTask);
                //if parent task is already in a milestone then do not assign task to another milestone
                if (parentTask.MilestoneId != null && !string.IsNullOrEmpty(taskCreateDto.Milestone))
                {
                    throw new AssignParentTaskException(MessageConstant.MilestoneFromParentAndFromChildrenError);
                }

                foreach (var childrenTask in existingChildrenTasks)
                {
                    if (taskCreateDto.ParentTask.Equals(childrenTask.Id))
                    {
                        throw new AssignParentTaskException(MessageConstant.CannotAssignChildrenTaskAsParent);
                    }
                }

                task.ParentTaskId = taskCreateDto.ParentTask;
                task.MilestoneId = parentTask.MilestoneId;

                foreach (var assignee in taskCreateDto.Assignees)
                {
                    var assigneeParentQueryTask = parentTask.UserTasks
                        .Where(ut => ut.TaskId.Equals(taskCreateDto.ParentTask) && ut.UserId.Equals(assignee)).ToList();
                    if (assigneeParentQueryTask.Count == 0)
                    {
                        await _taskRepository.AddUserToTask(assignee, taskCreateDto.ParentTask);
                    }
                }
            }

            try
            {
                _unitOfWork.BeginTransaction();

                foreach (var assignee in taskCreateDto.Assignees)
                {
                    task.UserTasks.Add(new UserTask
                    {
                        UserId = assignee,
                        TaskId = task.Id,
                        ActualManHour = 0,
                        LastUpdatedTime = DateTimeOffset.UtcNow
                    });
                }

                var taskEntity = _taskRepository.Add(task);
                string notification = userInProject.User.FullName + " đã tạo ra tác vụ: " + task.Title;
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

            if (chosenTask.Status != TaskEntityStatus.NOT_STARTED && chosenTask.Status != TaskEntityStatus.OPEN)
            {
                throw new UpdateException(MessageConstant.CannotUpdateTaskInfoWhenStarted);
            }

            if (chosenTask.ParentTaskId == null && userInProject.RoleInTeam != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }

            try
            {
                _unitOfWork.BeginTransaction();
                chosenTask.Title = updateTaskInfoDTO.Title;
                chosenTask.Description = updateTaskInfoDTO.Description;
                chosenTask.StartDate = updateTaskInfoDTO.StartDate;
                chosenTask.EndDate = updateTaskInfoDTO.EndDate;
                chosenTask.ManHour = updateTaskInfoDTO.ManHour ?? 0;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                chosenTask.Priority = updateTaskInfoDTO.Priority;
                TaskHistory history = new TaskHistory
                {
                    Content = $"{userInProject.User.FullName} đã cập nhật thông tin tác vụ {updateTaskInfoDTO.Title}",
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

            // filter tasks by create tim between start date and end date
            // if start date is null, filter tasks by create time before end date and vice versa
            if (taskFilterDto.StartDate != null && taskFilterDto.EndDate != null)
            {
                filterTasks = filterTasks.Where(t => t.CreatedTime >= taskFilterDto.StartDate && t.CreatedTime <= taskFilterDto.EndDate);
            }
            if (taskFilterDto.StartDate != null)
            {
                filterTasks = filterTasks.Where(t => t.CreatedTime >= taskFilterDto.StartDate);
            }
            if (taskFilterDto.EndDate != null)
            {
                filterTasks = filterTasks.Where(t => t.CreatedTime <= taskFilterDto.EndDate);
            }

            // order tasks by priority descending
            if (taskFilterDto.Priority != null)
            {
                if ((bool)taskFilterDto.Priority)
                {
                    filterTasks = filterTasks.OrderByDescending(t => t.Priority);
                }
                else
                {
                    filterTasks = filterTasks.OrderBy(t => t.Priority);
                }
            }

            if (taskFilterDto.isParentTask != null)
            {
                if ((bool)taskFilterDto.isParentTask)
                {
                    filterTasks = filterTasks.Where(t => t.ParentTaskId == null);
                }
                else
                {
                    filterTasks = filterTasks.Where(t => t.ParentTask != null);
                }

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
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                DeletedTime = task.DeletedTime,
                Description = task.Description,
                IsLate = task.IsLate,
                LastUpdatedBy = task.LastUpdatedBy,
                LastUpdatedTime = task.LastUpdatedTime,
                Status = task.Status,
                Title = task.Title,
                ExpectedManHour = task.ManHour
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
            var chosenTask = await _taskRepository.GetTaskDetails(taskId);
            if (chosenTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundTaskError);
            }

            if (chosenTask.ParentTaskId == null && userInProject.RoleInTeam != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }

            // cannot return to not started status in other task status
            if (updateTaskStatusDTO.Status == TaskEntityStatus.NOT_STARTED && chosenTask.Status != TaskEntityStatus.NOT_STARTED)
            {
                throw new UpdateException(MessageConstant.TaskAlreadyStartedError);
            }

            // this scenario is to check permission to re-open task
            if (updateTaskStatusDTO.Status == TaskEntityStatus.OPEN)
            {
                if (userInProject.RoleInTeam != RoleInTeam.Leader)
                {
                    throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
                }

                if (chosenTask.Status != TaskEntityStatus.DONE)
                {
                    throw new UpdateException(MessageConstant.CannotOpenTask);
                }
            }

            // check if status is to done and log time is not filled
            // this is only for children task since you cannot assign or log time into parent task
            if (chosenTask.ParentTaskId != null)
            {
                var userTask = chosenTask.UserTasks.FirstOrDefault(ut => ut.TaskId == taskId);

                if (userTask == null)
                {
                    throw new NotFoundException(MessageConstant.NotFoundAssigneeError);
                }

                if (updateTaskStatusDTO.Status == TaskEntityStatus.DONE && userTask.ActualManHour == 0)
                {
                    throw new UpdateException(MessageConstant.CannotCompleteTaskWithoutManHour);
                }

                if ((updateTaskStatusDTO.Status == TaskEntityStatus.DONE ||
                     updateTaskStatusDTO.Status == TaskEntityStatus.IN_PROGRESS ||
                     updateTaskStatusDTO.Status == TaskEntityStatus.PENDING)
                    && !userTask.UserId.Equals(userId))
                {
                    throw new UpdateException(MessageConstant.CannotChangeStatusTaskWrongAssignee);
                }
            }


            try
            {
                _unitOfWork.BeginTransaction();
                chosenTask.Status = updateTaskStatusDTO.Status;
                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
                // logic when task is done
                if (updateTaskStatusDTO.Status == TaskEntityStatus.DONE)
                {
                    chosenTask.ActualFinishAt = DateTimeOffset.UtcNow;
                    chosenTask.IsLate = false;
                }
                // logic when task is reopen
                if (updateTaskStatusDTO.Status == TaskEntityStatus.OPEN)
                {
                    chosenTask.ActualFinishAt = null;
                    chosenTask.IsLate = false;
                    chosenTask.StartDate = null;
                    chosenTask.EndDate = null;
                }

                TaskHistory history = new TaskHistory
                {
                    Content = userInProject.User.FullName + " đã cập nhật trạng thái tác vụ " + chosenTask.Title,
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

            if (chosenTask.Status != TaskEntityStatus.NOT_STARTED && chosenTask.Status != TaskEntityStatus.OPEN)
            {
                throw new UpdateException(MessageConstant.CannotUpdateTaskInfoWhenStarted);
            }

            var queryUserTask = chosenTask.UserTasks.FirstOrDefault(ct => ct.TaskId.Equals(taskId));
            if (queryUserTask != null)
            {
                throw new UpdateException(MessageConstant.NoMoreThanOneAssignee);
            }

            try
            {
                _unitOfWork.BeginTransaction();
                chosenTask.UserTasks.Add(new UserTask
                {
                    UserId = updateTaskAssignmentDTO.AssigneeId,
                    TaskId = taskId,
                    ActualManHour = 0
                });

                if (chosenTask.ParentTaskId != null)
                {
                    var parentTask = await _taskRepository.GetTaskDetails(chosenTask.ParentTaskId);
                    var userParentTask = parentTask.UserTasks.Where(ut => ut.TaskId.Equals(parentTask.Id) && ut.UserId.Equals(updateTaskAssignmentDTO.AssigneeId));
                    if (!userParentTask.Any())
                    {
                        await _taskRepository.AddUserToTask(updateTaskAssignmentDTO.AssigneeId, chosenTask.ParentTaskId);
                    }

                }
                chosenTask.LastUpdatedBy = userInProject.User.FullName;
                chosenTask.LastUpdatedTime = DateTimeOffset.UtcNow;
                TaskHistory history = new TaskHistory
                {
                    Content = $"{userInProject.User.FullName} đã phân công {assigneeInProject.User.FullName} vào tác vụ {chosenTask.Title}",
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

            if (chosenTask.Status != TaskEntityStatus.NOT_STARTED && chosenTask.Status != TaskEntityStatus.OPEN)
            {
                throw new UpdateException(MessageConstant.CannotUpdateTaskInfoWhenStarted);
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
                    Content = $"{userInProject.User.FullName} đã hủy phân công {assigneeInProject.User.FullName} khỏi tác vụ {chosenTask.Title}",
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
                        Content = $"{userInProject.User.FullName} đã gán tác vụ {chosenTask.Title} vào cột mốc {chosenMilestone.Title}",
                        CreatedBy = userInProject.User.FullName,
                        TaskId = chosenTask.Id
                    };
                    _taskHistoryRepository.Add(history);
                }
                else
                {
                    TaskHistory history = new TaskHistory
                    {
                        Content = $"{userInProject.User.FullName} đã hủy gán tác vụ {chosenTask.Title} vào cột mốc",
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
                    Content = $"{userInProject.User.FullName} đã hủy gán tác vụ {chosenTask.Title} vào tác vụ cha",
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
                    Content = $"{userInProject.User.FullName} đã gán tác vụ {chosenTask.Title} vào tác vụ cha {parentTask.Title}",
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

            // if task is not done or not started then throw error
            if (chosenTask.Status != TaskEntityStatus.NOT_STARTED && chosenTask.Status != TaskEntityStatus.OPEN)
            {
                throw new UpdateException(MessageConstant.CannotDeleteTaskWhenStarted);
            }

            try
            {
                _unitOfWork.BeginTransaction();
                TaskHistory history = new TaskHistory
                {
                    Content = "",
                    CreatedBy = userInProject.User.FullName,
                    TaskId = chosenTask.Id
                };
                await _taskRepository.SoftDeleteById(taskId);
                // if task is parent task then also delete all sub tasks, while also add task history
                if (chosenTask.ParentTaskId == null)
                {
                    var subTasks = await _taskRepository.QueryHelper().Filter(t => t.ParentTaskId == taskId).GetAllAsync();
                    history.Content = userInProject.User.FullName + " đã xóa tác vụ mẹ "
                        + chosenTask.Title + " cùng với " + subTasks.Count() + " tác vụ con";
                    foreach (var subTask in subTasks)
                    {
                        await _taskRepository.SoftDeleteById(subTask.Id);
                    }
                }

                if (chosenTask.ParentTaskId != null)
                {
                    history.Content = userInProject.User.FullName + " đã xóa tác vụ con " + chosenTask.Title;
                }
                _taskHistoryRepository.Add(history);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception(MessageConstant.DeleteFailed);
            }
        }

        public async Task MarkTaskAsLate()
        {
            var tasks = await _taskRepository.QueryHelper()
                .Filter(c => c.IsLate == false && c.Status == TaskEntityStatus.IN_PROGRESS && c.EndDate < DateTimeOffset.UtcNow)
                .GetAllAsync();
            foreach (var task in tasks)
            {
                task.IsLate = true;
                _taskRepository.Update(task);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkTaskAsStartLate()
        {
            var tasks = await _taskRepository.QueryHelper()
                .Filter(c => c.IsLate == false && c.Status == TaskEntityStatus.NOT_STARTED &&
                             c.StartDate <= DateTimeOffset.UtcNow
                             && c.UserTasks.Count() == 0).GetAllAsync();
            foreach (var task in tasks)
            {
                task.IsLate = true;
                _taskRepository.Update(task);
            }

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task StartTask()
        {
            var tasks = await _taskRepository.QueryHelper()
                .Filter(c =>
                    c.IsLate == false &&
                    (c.Status == TaskEntityStatus.NOT_STARTED && c.StartDate <= DateTimeOffset.UtcNow)
                    && c.UserTasks.Any(t => t.UserId != null)).GetAllAsync();
            foreach (var task in tasks)
            {
                task.Status = TaskEntityStatus.IN_PROGRESS;
                _taskRepository.Update(task);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateManHourForTask(string projectId, string taskId, string userId, float hour)
        {
            var userProject = await _userService.CheckIfUserInProject(userId, projectId);
            // check if task is parent task, if so then throw error cannot update man hour of parent task
            var task = await _taskRepository.GetOneAsync(taskId);
            if (task.ParentTaskId == null)
            {
                throw new UpdateException(MessageConstant.CannotUpdateManHourOfParentTask);
            }

            // check if status is not in progress then throw error cannot update
            if (task.Status != TaskEntityStatus.IN_PROGRESS && task.Status != TaskEntityStatus.PENDING)
            {
                throw new UpdateException(MessageConstant.CannotUpdateManHourWhenNotInProgress);
            }
            await _taskRepository.UpdateManHourForTask(taskId, userId, hour);
        }

        public async Task<float> GetManHoursForTask(string projectId, string userId, string taskId)
        {
            var userProject = await _userService.CheckIfUserInProject(userId, projectId);
            return await _taskRepository.GetManHoursForTask(taskId);
        }

        public async Task<TasksForUserDTO> GetAllTasksInformationOfUser(string userId, string projectId)
        {
            var tasks = await _taskRepository.GetAllUserTasksInOneProject(userId, projectId);
            var response = new TasksForUserDTO();

            foreach (var task in tasks)
            {
                response.ActualManHourInProject += task.ActualManHour;
            }

            response.DoneTasks = tasks.Count(t => t.Task.Status == TaskEntityStatus.DONE);
            response.NotStartedTasks = tasks.Count(t => t.Task.Status == TaskEntityStatus.NOT_STARTED);
            response.PendingTasks = tasks.Count(t => t.Task.Status == TaskEntityStatus.PENDING);
            response.InProgressTasks = tasks.Count(t => t.Task.Status == TaskEntityStatus.IN_PROGRESS);
            return response;
        }

        public async Task<List<AllTaskHistoryForUserDTO>> GetAllTaskHistoryForUser(string userId)
        {
            var userProjects = await _userService.GetProjectsByUserId(userId);
            var eachProjectResponse = new AllTaskHistoryForUserDTO();
            var response = new List<AllTaskHistoryForUserDTO>();
            var userTaskHistory = new List<UserTaskInTaskHistoryDTO>();
            foreach (var userProject in userProjects)
            {
                var userTasksQuery = await _taskRepository.GetAllUserTasksInOneProject(userId, userProject.ProjectId);
                foreach (var userTask in userTasksQuery)
                {
                    eachProjectResponse.TotalManHoursInProject += userTask.ActualManHour;
                    var task = new UserTaskInTaskHistoryDTO
                    {
                        Title = userTask.Task.Title,
                        ActualManHour = userTask.ActualManHour,
                        Status = userTask.Task.Status
                    };
                    userTaskHistory.Add(task);
                }
                eachProjectResponse.ProjectName = userProject.Project.ProjectName;
                eachProjectResponse.UserStatusInProject = userProject.Status;
                eachProjectResponse.UserTasks = userTaskHistory;
            }

            response.Add(eachProjectResponse);
            return response;
        }
    }
}
