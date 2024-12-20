using Microsoft.EntityFrameworkCore;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories;

public class RecruitmentRepository : GenericRepository<Recruitment, string>, IRecruitmentRepository
{
    private readonly AppDbContext _appDbContext;

    public RecruitmentRepository(AppDbContext context) : base(context)
    {
        _appDbContext = context;
    }

    public async Task<Recruitment> GetRecruitmentPostByProjectId(string projectId)
    {
        var recruitment =
            await _dbSet.Where(r => r.ProjectId.Equals(projectId))
                .Include(r => r.RecruitmentImgs)
                .Include(r => r.Project)
                .ThenInclude(p => p.UserProjects)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync();
        return recruitment;
    }

    public async Task<Recruitment> GetRecruitmentPostByRecruitmentId(string recruitmentId)
    {
        var recruitment =
            await _dbSet.Where(r => r.Id.Equals(recruitmentId))
                .Include(r => r.RecruitmentImgs)
                .Include(r => r.Project)
                .ThenInclude(p => p.UserProjects)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync();
        return recruitment;
    }

    public IQueryable<Recruitment> GetRecruitmentWithLeader()
    {
        var recruitments = _dbSet.Include(r => r.Project).ThenInclude(p => p.UserProjects)
            .ThenInclude(up => up.User).Where(r => r.IsOpen == true);
        return recruitments;
    }
}