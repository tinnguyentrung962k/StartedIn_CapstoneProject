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
    public class TerminationConfirmRepository : GenericRepository<TerminationConfirmation, string>, ITerminationConfirmRepository
    {
        private readonly AppDbContext _appDbContext;
        public TerminationConfirmRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public async Task<List<TerminationConfirmation>> GetPendingTerminationConfirmationForUserInProject(string userId, string projectId)
        {
            var confirmList = await _appDbContext.TerminationConfirmations
                .Include(r => r.TerminationRequest)
                .ThenInclude( r => r.Contract)
                .ThenInclude( r => r.UserContracts)
                .ThenInclude( r => r.User)
                .Where(r => r.TerminationRequest.Contract.ProjectId.Equals(projectId) && r.ConfirmUserId == userId && r.IsAgreed == null)
                .ToListAsync();
            return confirmList;
        }
    }
}
