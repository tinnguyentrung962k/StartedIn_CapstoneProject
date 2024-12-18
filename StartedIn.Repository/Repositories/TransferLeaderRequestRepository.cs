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
    public class TransferLeaderRequestRepository : GenericRepository<TransferLeaderRequest, string>, ITransferLeaderRequestRepository
    {
        private readonly AppDbContext _appDbContext;
        public TransferLeaderRequestRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public async Task<TransferLeaderRequest> GetLeaderTransferRequestPending(string projectId)
        {
            var request = await _appDbContext.TransferLeaderRequests
                .Where(r => r.ProjectId == projectId && r.IsAgreed == null)
                .Include(r => r.Appointment)
                .ThenInclude(r => r.MeetingNotes)
                .Include(r => r.FormerLeader).FirstOrDefaultAsync();
            return request;
        }
    }
}
