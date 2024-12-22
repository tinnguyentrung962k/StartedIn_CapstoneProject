using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface ITransferLeaderRequestRepository : IGenericRepository<TransferLeaderRequest,string>
    {
        Task<TransferLeaderRequest> GetLeaderTransferRequestPending(string projectId);
        IQueryable<TransferLeaderRequest> GetHistoryOfLeaderInAProject(string projectId);
        Task<TransferLeaderRequest> GetLeaderTransferInProjectById(string transferId);
    }
}
