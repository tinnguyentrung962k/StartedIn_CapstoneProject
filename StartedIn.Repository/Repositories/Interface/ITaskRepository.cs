using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface ITaskRepository : IGenericRepository<TaskEntity, string>
    {
        Task<TaskEntity> GetTaskDetails(string taskId);
        Task<List<TaskEntity>> GetSubTaskDetails(string parentTaskId);
        IQueryable<TaskEntity> GetTaskListInAProjectQuery(string projectId);
        Task UpdateManHourForTask(string taskId, string userId, float hour);
        Task<float> GetManHoursForTask(string taskId);
        Task<List<UserTask>> GetAllUserTasksInOneProject(string userId, string projectId);
        Task<List<TaskEntity>> GetAllTaskEntitiesOfUserInOneProject(string projectId, string userId);
        Task AddUserToTask(string userId, string taskId);
    }
}
