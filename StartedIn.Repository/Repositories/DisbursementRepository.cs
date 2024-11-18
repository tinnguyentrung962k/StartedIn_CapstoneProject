using Microsoft.EntityFrameworkCore;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories
{
    public class DisbursementRepository : GenericRepository<Disbursement, string>, IDisbursementRepository
    {
        private readonly AppDbContext _appDbContext;
        public DisbursementRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }
        public async Task<bool> ExistsAsync(long orderCode)
        {
            return await _appDbContext.Disbursements.AnyAsync(b => b.OrderCode == orderCode);
        }

        public async Task<Disbursement> GetDisbursementById(string id)
        {
            return await _appDbContext.Disbursements.Where(x=>x.Id.Equals(id))
                .Include(d=>d.Contract)
                .ThenInclude(c=>c.Project)
                .Include(d=>d.Investor)
                .Include(d=>d.DisbursementAttachments)
                .Where(x => x.DeletedTime == null && x.IsValidWithContract == true)
                .FirstOrDefaultAsync();
        }
    }
}
