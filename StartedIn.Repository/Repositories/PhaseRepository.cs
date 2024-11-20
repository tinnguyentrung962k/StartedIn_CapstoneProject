using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories;

public class PhaseRepository : GenericRepository<Phase, string>, IPhaseRepository
{
    private readonly AppDbContext _appDbContext;
    public PhaseRepository(AppDbContext context) : base(context)
    {
        _appDbContext = context;
    }
}