using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories;

public class RecruitmentImageRepository : GenericRepository<RecruitmentImg, string>, IRecruitmentImageRepository
{
    private readonly AppDbContext _appDbContext;
    
    public RecruitmentImageRepository(AppDbContext context) : base(context)
    {
        _appDbContext = context;
    }
}