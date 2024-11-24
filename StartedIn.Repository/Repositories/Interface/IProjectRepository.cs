using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Interface;

public interface IProjectRepository : IGenericRepository<Project, string>
{ 
    Task<Project> GetProjectById(string id);
    Task<Project> GetProjectAndMemberByProjectId(string projectId);
    Task<RoleInTeam> GetUserRoleInProject(string userId, string projectId);
    IQueryable<Project> GetProjectListQuery();
    
}