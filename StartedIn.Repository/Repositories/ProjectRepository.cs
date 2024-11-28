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
            .Include(p => p.UserProjects)
            .ThenInclude(up => up.User)
            .FirstOrDefaultAsync();
        return project;
    }

    public async Task<Project> GetProjectById(string id)
    {
        var project = await _appDbContext.Projects.Where(p => p.Id.Equals(id))
            .Include(p => p.UserProjects)
            .ThenInclude(up => up.User)
            .Include(p => p.InvestmentCalls)
            .ThenInclude(p=>p.DealOffers)
            .Include(p => p.Finance)
            .Include(p => p.ProjectCharter)
            .ThenInclude(pc => pc.Phases)
            .ThenInclude(p => p.Milestones)
            .ThenInclude(x => x.Tasks)
            .Include(p => p.Contracts)
            .ThenInclude(c => c.Disbursements)
            .Where(x => x.DeletedTime == null)
            .FirstOrDefaultAsync();
        return project;
    }

    public IQueryable<Project> GetProjectListQuery()
    {
        var projects = _appDbContext.Projects
            .Include(p => p.UserProjects)
            .ThenInclude(up => up.User)
            .Include(p => p.InvestmentCalls)
            .Include(p => p.Finance)
            .Include(p => p.ProjectCharter)
            .ThenInclude(pc => pc.Phases)
            .Include(p => p.Contracts)
            .ThenInclude(c => c.Disbursements)
            .Include(x => x.Milestones)
            .ThenInclude(x => x.Tasks)
            .Where(x => x.DeletedTime == null);
        return projects;
    }

    public async Task<RoleInTeam> GetUserRoleInProject(string userId, string projectId)
    {
        var userProject = await _appDbContext.UserProjects.Where(x => x.ProjectId.Equals(projectId) && x.UserId.Equals(userId)).FirstOrDefaultAsync();
        return userProject.RoleInTeam;
    }

    public async Task<Project> GetProjectWithOnlyLeader(string projectId)
    {
        var project = await _appDbContext.Projects.Where(p => p.Id.Equals(projectId))
            .Include(p => p.UserProjects)
            .ThenInclude(up => up.User)
            .Where(p => p.UserProjects.Any(up => up.RoleInTeam == RoleInTeam.Leader))
            .FirstOrDefaultAsync();
        return project;
    }

    public async Task<UserProject> GetAProjectByUserId(string userId)
    {
        var userProject = await _appDbContext.UserProjects
            .Where(p => p.UserId.Equals(userId) && p.RoleInTeam.Equals(RoleInTeam.Leader) || p.RoleInTeam.Equals(RoleInTeam.Member))
            .FirstOrDefaultAsync();
        return userProject;
    }

}