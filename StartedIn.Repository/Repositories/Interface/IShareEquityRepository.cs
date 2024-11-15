using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface IShareEquityRepository : IGenericRepository <ShareEquity,string>
    {
        Task<List<ShareEquity>> GetShareEquityOfMembersInAProject(string projectId);
    }
}
