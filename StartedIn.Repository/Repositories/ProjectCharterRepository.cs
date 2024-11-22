using Microsoft.EntityFrameworkCore;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;


namespace StartedIn.Repository.Repositories
{
    public class ProjectCharterRepository : GenericRepository<ProjectCharter, string>, IProjectCharterRepository
    {
        private readonly AppDbContext _appDbContext;

        public ProjectCharterRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public async Task<ProjectCharter> GetProjectCharterByCharterId(string charterId)
        {
            var projectCharter = await _appDbContext.ProjectCharters
                    .Include(pc => pc.Phases)
                    .Where(x => x.DeletedTime == null)
                    .FirstOrDefaultAsync(p => p.Id.Equals(charterId));
            return projectCharter;
        }

        public async Task<ProjectCharter> GetProjectCharterByProjectId(string projectId)
        {
            var projectCharter = await _appDbContext.ProjectCharters
                .Include(pc => pc.Phases)
                .Where(x => x.DeletedTime == null)
                .FirstOrDefaultAsync(p => p.ProjectId.Equals(projectId));
            return projectCharter;
        }
    }
}
