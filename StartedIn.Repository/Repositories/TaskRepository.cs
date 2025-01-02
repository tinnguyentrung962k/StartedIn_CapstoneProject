using Microsoft.EntityFrameworkCore;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories
{
    public class TaskRepository : GenericRepository<TaskEntity, string>, ITaskRepository
    {
        private readonly AppDbContext _appDbContext;
        public TaskRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public Task<TaskEntity> GetTaskDetails(string taskId)
        {
            return _dbSet.Include(x => x.Milestone)
                .Include(x => x.UserTasks).ThenInclude(x => x.User)
                .Include(x => x.ParentTask)
                .Include(t => t.TaskComments).ThenInclude(t => t.User)
                .Include(t => t.TaskAttachments)
                .Where(x => x.DeletedTime == null)
                .FirstOrDefaultAsync(x => x.Id == taskId);
        }
        public IQueryable<TaskEntity> GetTaskListInAProjectQuery(string projectId)
        {
            var query = _dbSet.Include(t => t.UserTasks)
                .ThenInclude(ut => ut.User)
                .Where(t => t.ProjectId.Equals(projectId) && t.DeletedTime == null).OrderBy(t => t.CreatedTime);
            return query;
        }

        public async Task UpdateManHourForTask(string taskId, string userId, float hour)
        {
            var queryTask = _appDbContext.UserTasks.FirstOrDefault(ut => ut.TaskId.Equals(taskId) && ut.UserId.Equals(userId));
            if (queryTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundUserTask);
            }
            queryTask.ActualManHour = hour; 
            queryTask.LastUpdatedTime = DateTimeOffset.UtcNow;
            _appDbContext.Set<UserTask>().Update(queryTask);
            await _context.SaveChangesAsync();
        }

        public async Task<float> GetManHoursForTask(string taskId)
        {
            var queryTask = await _appDbContext.UserTasks.Where(ut => ut.TaskId.Equals(taskId)).ToListAsync();
            if (queryTask == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundUserTask);
            }

            float manHour = 0;
            foreach (var task in queryTask)
            {
                manHour += task.ActualManHour;
            }
            return manHour;
        }

        public async Task<List<UserTask>> GetAllUserTasksInOneProject(string userId, string projectId)
        {
            var queryTasks = await _appDbContext.UserTasks.Where(ut => ut.UserId.Equals(userId))
                .Include(ut => ut.Task).ThenInclude(t => t.Project).ToListAsync();
             
            if (!queryTasks.Any())
            {
                throw new NotFoundException(MessageConstant.NotFoundUserTask);
            }
            
            var queryTasksInProject = new List<UserTask>();
            foreach (var task in queryTasks)
            {
                if (task.Task.ProjectId.Equals(projectId))
                {
                    queryTasksInProject.Add(task);
                }
            }
            return queryTasksInProject;
        }
        
        public async Task<List<TaskEntity>> GetAllTaskEntitiesOfUserInOneProject(string projectId, string userId)
        {
            var tasks = await _appDbContext.TaskEntities.Where(t =>
                t.ProjectId.Equals(projectId) && t.UserTasks.Any(ut => ut.UserId.Equals(userId))).ToListAsync();
            return tasks;
        }
        
        public async Task AddUserToTask(string userId, string taskId)
        {
            var userTask = new UserTask
            {
                UserId = userId,
                TaskId = taskId,
                ActualManHour = 0,
                LastUpdatedTime = DateTimeOffset.UtcNow
            };
            await _appDbContext.Set<UserTask>().AddAsync(userTask);
        }
        
    }
}
