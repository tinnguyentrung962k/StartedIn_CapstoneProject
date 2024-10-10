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
    public class MilestoneRepository : GenericRepository<Milestone, string>, IMilestoneRepository
    {
        private readonly AppDbContext _appDbContext;
        public MilestoneRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public async Task<Milestone> GetMilestoneDetailById(string milestoneId)
        {
            var milestone = await _appDbContext.Milestones
                .Include(p => p.Taskboards)
                .ThenInclude(t => t.TasksList)
                .FirstOrDefaultAsync(p => p.Id.Equals(milestoneId));
            return milestone;
        }
    }
}
