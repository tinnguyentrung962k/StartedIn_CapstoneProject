using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface ITerminationConfirmRepository : IGenericRepository<TerminationConfirmation,string>
    {
        Task<List<TerminationConfirmation>> GetTerminationConfirmationForUserInProject(string userId, string projectId);
    }
}
