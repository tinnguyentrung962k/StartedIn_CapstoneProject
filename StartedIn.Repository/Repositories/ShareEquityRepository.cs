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
    public class ShareEquityRepository : GenericRepository<ShareEquity, string>, IShareEquityRepository
    {
        private readonly AppDbContext _appDbContext;
        public ShareEquityRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public async Task<IQueryable<ShareEquity>> GetShareEquityOfMembersInAProject(string projectId)
        {
             var newestShareEquity = _appDbContext.ShareEquities
            .Where(x => x.Contract.ProjectId.Equals(projectId) && x.DateAssigned != null)
            .Include(x => x.Contract)
            .ThenInclude(x => x.UserContracts)
            .Include(x => x.User);
            return newestShareEquity;
        }
    }
}
