using Microsoft.EntityFrameworkCore;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories
{
    public class TaskRepository : GenericRepository<TaskEntity, string>, ITaskRepository
    {
        public TaskRepository(AppDbContext context) : base(context)
        {
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
    }
}
