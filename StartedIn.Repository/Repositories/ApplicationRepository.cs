using Microsoft.EntityFrameworkCore;
using StartedIn.CrossCutting.Enum;
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
    public class ApplicationRepository : GenericRepository<Application, string>, IApplicationRepository
    {
        public ApplicationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Application>> GetApplicationsWithCandidate(string projectId)
        {
            return await _dbSet.Include(x => x.Candidate).ThenInclude(c => c.Applications.Take(0)).Where(x => x.ProjectId.Equals(projectId)
                                            && x.Type == ApplicationTypeEnum.APPLY
                                                                        && x.Status == ApplicationStatus.PENDING).ToListAsync();
        }
    }
}
