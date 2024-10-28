using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface IProjectCharterRepository : IGenericRepository<ProjectCharter, string>
    {
        Task<ProjectCharter> GetProjectCharterByCharterId(string charterId);
        Task<ProjectCharter> GetProjectCharterByProjectId(string projectId);
    }
}
