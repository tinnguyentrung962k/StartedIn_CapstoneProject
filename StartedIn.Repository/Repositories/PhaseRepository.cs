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
    public class PhaseRepository : GenericRepository<Phase, string>, IPhaseRepository
    {
        private readonly AppDbContext _appDbContext;
        public PhaseRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public async Task<Phase> GetPhaseDetailById(string id)
        {
            var phase = await _appDbContext.Phases
                .Include(p => p.Milestones.OrderBy(p=>p.MilestoneDate))
                .ThenInclude(t => t.Taskboards)
                .ThenInclude(t=>t.TasksList)
                .FirstOrDefaultAsync(p => p.Id.Equals(id));
            return phase;
        }
    }
}
