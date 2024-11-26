using Microsoft.EntityFrameworkCore;
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

    public async Task<List<InvestmentCall>> GetInvestmentCallByProjectId(string projectId)
    {
        var list = await _appDbContext.InvestmentCalls.Where(ic => ic.ProjectId.Equals(projectId)).Include(ic => ic.DealOffers)
            .ThenInclude(d => d.Investor).ToListAsync();
        return list;
    }
}