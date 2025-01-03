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
            .Include(p => p.UserProjects.Where(x => x.Status == UserStatusInProject.Active))
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
            .Include(p => p.ProjectApprovals)
            .ThenInclude(p => p.Documents)
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
            .Where(p => p.UserId.Equals(userId) &&
                        (p.RoleInTeam.Equals(RoleInTeam.Leader) || p.RoleInTeam.Equals(RoleInTeam.Member)) 
                        && p.Status == UserStatusInProject.Active)
            .FirstOrDefaultAsync();
        return userProject;
    }
    public IQueryable<Project> GetProjectListQueryForInvestor(string userId)
    {
        var query = _appDbContext.Projects
            .Include(p => p.UserProjects.Where(x => x.Status == UserStatusInProject.Active))
            .ThenInclude(up => up.User)
            .Include(p => p.InvestmentCalls)
            .Include(p => p.Finance)
            .Include(p => p.ProjectCharter)
            .ThenInclude(pc => pc.Phases)
            .Include(p => p.Contracts)
            .ThenInclude(c => c.Disbursements)
            .Include(x => x.Milestones)
            .ThenInclude(x => x.Tasks)
            .Where(x => x.DeletedTime == null)
            .Where(p => !p.UserProjects.Any(up => up.UserId.Contains(userId)) && p.ProjectStatus.Equals(ProjectStatusEnum.ACTIVE))
            .OrderByDescending(x => x.StartDate);
        return query;
    }

    public IQueryable<Project> GetClosedProjectsForUser(string userId)
    {
        var projects = _appDbContext.Projects.Include(p => p.UserProjects)
            .Where(p => p.UserProjects.Any(up => up.UserId == userId) && p.ProjectStatus == ProjectStatusEnum.CLOSED);
        return projects;
    }

    public IQueryable<Project> GetProjectsThatUserLeft(string userId)
    {
        var projects = _appDbContext.Projects.Include(p => p.UserProjects)
            .Where(p => p.UserProjects.Any(up => up.UserId == userId && up.Status == UserStatusInProject.Left));
        return projects;
    }

    public async Task<UserStatusInProject> GetUserStatusInProject(string userId, string projectId)
    {
        var userProject = await _appDbContext.UserProjects.Where(x => x.ProjectId.Equals(projectId) && x.UserId.Equals(userId)).FirstOrDefaultAsync();
        return userProject.Status;
    }
}