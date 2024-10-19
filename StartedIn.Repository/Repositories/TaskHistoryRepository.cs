using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories;

public class TaskHistoryRepository : GenericRepository<TaskHistory, string>, ITaskHistoryRepository 
{
    public TaskHistoryRepository(AppDbContext context) : base(context)
    {
    }
}