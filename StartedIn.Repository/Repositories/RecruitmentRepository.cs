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

    public async Task<Recruitment> GetRecruitmentPostById(string projectId, string recruitmentId)
    {
        var recruitment =
            await _appDbContext.Recruitments.Where(r => r.ProjectId.Equals(projectId) && r.Id.Equals(recruitmentId)).Include(r => r.RecruitmentImgs).FirstOrDefaultAsync();
        return recruitment;
    }
}