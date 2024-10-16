using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Interface;

public interface IProjectRepository : IGenericRepository<Project, string>
{ 
    Task<Project> GetProjectById(string id);
}