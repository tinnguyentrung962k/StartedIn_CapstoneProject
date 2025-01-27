using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Interface;

public interface IInvestmentCallRepository : IGenericRepository<InvestmentCall, string>
{
    IQueryable<InvestmentCall> GetInvestmentCallByProjectId(string projectId);
}