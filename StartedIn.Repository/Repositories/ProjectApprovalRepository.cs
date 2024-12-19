using Microsoft.EntityFrameworkCore;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories;

public class ProjectApprovalRepository : GenericRepository<ProjectApproval, string>, IProjectApprovalRepository
{
    private readonly AppDbContext _appDbContext;
    public ProjectApprovalRepository(AppDbContext context) : base(context)
    {
        _appDbContext = context;
    }

    public IQueryable<ProjectApproval> GetProjectApprovalsQuery()
    {
        var projectApprovals = _appDbContext.ProjectApprovals
            .Include(pa => pa.Project).ThenInclude(p => p.UserProjects).ThenInclude(up => up.User)
            .Where(x => x.DeletedTime == null);
        return projectApprovals;
    }
}