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
    public class LeavingRequestRepository : GenericRepository<LeavingRequest, string>, ILeavingRequestRepository
    {
        private readonly AppDbContext _appDbContext;
        public LeavingRequestRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public IQueryable<LeavingRequest> GetLeavingRequestForLeaderInProject(string projectId)
        {
            var query = _appDbContext.LeavingRequests.Where(x => x.ProjectId.Equals(projectId))
                .Include(x => x.Project)
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedTime);
            return query;
        }
    }
}
