using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface ITaskRepository : IGenericRepository<TaskEntity, string>
    {
        Task<TaskEntity> GetTaskDetails(string taskId);
        IQueryable<TaskEntity> GetTaskListInAProjectQuery(string projectId);
    }
}
