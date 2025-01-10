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
        IQueryable<Disbursement> GetDisbursementListOfInvestorQuery(string userId);
        IQueryable<Disbursement> GetDisbursementListOfAProjectQuery(string projectId);
        IQueryable<Disbursement> GetDisbursementListOfAnInvestorInAProjectQuery(string userId, string projectId);
        Task RemoveDisbursementAttachments(string disbursementId);

    }
}
