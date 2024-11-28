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
}