using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface IPhaseRepository : IGenericRepository<Phase, string>
    {
        Task<Phase> GetPhaseDetailById(string id);

    }
}
