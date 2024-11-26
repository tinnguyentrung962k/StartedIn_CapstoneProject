using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Interface;

public interface IInvestmentCallRepository : IGenericRepository<InvestmentCall, string>
{
    public Task<List<InvestmentCall>> GetInvestmentCallByProjectId(string projectId);
}