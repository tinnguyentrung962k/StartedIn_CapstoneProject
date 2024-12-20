using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Interface;

public interface IProjectRepository : IGenericRepository<Project, string>
{ 
    Task<Project> GetProjectById(string id);
    Task<Project> GetProjectAndMemberByProjectId(string projectId);
    Task<RoleInTeam> GetUserRoleInProject(string userId, string projectId);
    Task<UserStatusInProject> GetUserStatusInProject(string userId, string projectId); 
    Task<Project> GetProjectWithOnlyLeader(string projectId);
    IQueryable<Project> GetProjectListQuery();
    Task<UserProject> GetAProjectByUserId(string userId);
    IQueryable<Project> GetProjectListQueryForInvestor(string userId);
    IQueryable<Project> GetClosedProjectsForUser(string userId);
    IQueryable<Project> GetProjectsThatUserLeft(string userId);
    
}