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
                .FirstOrDefaultAsync(x => x.Id == taskId);
        }
    }
}
