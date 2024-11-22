using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface IMilestoneRepository : IGenericRepository<Milestone, string>
    {
        Task<Milestone> GetMilestoneDetailById(string milestoneId);
        IQueryable<Milestone> GetMilestoneListQuery(string projectId);
    }
}
