using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface ITerminationRequestRepository : IGenericRepository<TerminationRequest,string>
    {
        Task<List<TerminationRequest>> GetTerminationRequestForUserInProject(string userId, string projectId);
    }
}
