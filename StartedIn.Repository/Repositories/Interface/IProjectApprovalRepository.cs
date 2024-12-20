using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Interface;

public interface IProjectApprovalRepository : IGenericRepository<ProjectApproval, string>
{
    IQueryable<ProjectApproval> GetProjectApprovalsQuery();
}