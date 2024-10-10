using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IPhaseService
    {
        Task<Phase> GetPhaseDetailById(string id);
    }
}
