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

    public IQueryable<InvestmentCall> GetInvestmentCallByProjectId(string projectId)
    {
        var investmentCallsQuery = _appDbContext.InvestmentCalls
            .Where(ic => ic.ProjectId.Equals(projectId) && ic.DeletedTime == null)
            .Include(ic => ic.DealOffers.Where(x => x.DeletedTime == null))
            .ThenInclude(d => d.Investor);
        return investmentCallsQuery;
    }
}