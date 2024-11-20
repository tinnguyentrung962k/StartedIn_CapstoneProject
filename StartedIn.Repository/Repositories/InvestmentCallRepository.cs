using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories;

public class InvestmentCallRepository : GenericRepository<InvestmentCall, string>, IInvestmentCallRepository
{
    private readonly AppDbContext _appDbContext;
    
    public InvestmentCallRepository(AppDbContext context) : base(context)
    {
        _appDbContext = context;
    }
}