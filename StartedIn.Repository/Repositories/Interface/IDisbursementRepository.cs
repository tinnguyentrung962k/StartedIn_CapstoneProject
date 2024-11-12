using Microsoft.EntityFrameworkCore;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface IDisbursementRepository : IGenericRepository<Disbursement,string>
    {
        Task<bool> ExistsAsync(long orderCode);
        Task<Disbursement> GetDisbursementById(string id);
      
    }
}
