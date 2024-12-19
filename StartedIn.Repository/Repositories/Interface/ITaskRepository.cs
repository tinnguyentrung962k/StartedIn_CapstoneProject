using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface ITaskRepository : IGenericRepository<TaskEntity, string>
    {
        Task<TaskEntity> GetTaskDetails(string taskId);
        IQueryable<TaskEntity> GetTaskListInAProjectQuery(string projectId);
        Task UpdateManHourForTask(string taskId, string userId, float hour);
        Task<float> GetManHoursForTask(string taskId);
        Task<List<UserTask>> GetAllTasksOfUserInOneProject(string userId, string projectId);
    }
}
