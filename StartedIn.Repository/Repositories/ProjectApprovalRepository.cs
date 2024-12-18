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
}