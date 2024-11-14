using Microsoft.EntityFrameworkCore;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories;

public class ProjectRepository : GenericRepository<Project, string>, IProjectRepository
{
    private readonly AppDbContext _appDbContext;
    
    public ProjectRepository(AppDbContext context) : base(context)
    {
        _appDbContext = context;
    }

    public async Task<Project> GetProjectAndMemberByProjectId(string projectId)
    {
        var project = await _appDbContext.Projects.Where(p => p.Id.Equals(projectId))
            .Include(p=>p.UserProjects)
            .ThenInclude(up => up.User)
            .FirstOrDefaultAsync();
        return project;
    }

    public async Task<Project> GetProjectById(string id)
    {
        var project = await _appDbContext.Projects.Where(p => p.Id.Equals(id))
            .FirstOrDefaultAsync();
        return project;
    }

    public async Task<RoleInTeam> GetUserRoleInProject(string userId, string projectId)
    {
        var userProject = await _appDbContext.UserProjects.Where(x => x.ProjectId.Equals(projectId) && x.UserId.Equals(userId)).FirstOrDefaultAsync();
        return userProject.RoleInTeam;
    }

}