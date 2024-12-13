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
    public class TerminationRequestRepository : GenericRepository<TerminationRequest, string>, ITerminationRequestRepository
    {
        private readonly AppDbContext _appDbContext;
        public TerminationRequestRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public async Task<List<TerminationRequest>> GetTerminationRequestForFromUserInProject(string userId, string projectId)
        {
            var requestList = await _appDbContext.TerminationRequests
                .Include(r => r.Contract)
                .ThenInclude(c => c.UserContracts)
                .ThenInclude(uc => uc.User)
                .Where(r => r.Contract.ProjectId.Equals(projectId) && r.FromId == userId)
                .ToListAsync();
            return requestList;
        }

        public async Task<List<TerminationRequest>> GetTerminationRequestForToUserInProject(string userId, string projectId)
        {
            var requestList = await _appDbContext.TerminationRequests
                .Include(r => r.Contract)
                .ThenInclude(c => c.UserContracts)
                .ThenInclude(uc => uc.User)
                .Where(r => r.Contract.ProjectId.Equals(projectId) && r.ToId == userId)
                .ToListAsync();
            return requestList;
        }
    }
}
