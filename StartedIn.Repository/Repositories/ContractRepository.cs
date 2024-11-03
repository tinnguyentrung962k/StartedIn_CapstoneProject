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
    public class ContractRepository : GenericRepository<Contract, string>, IContractRepository
    {
        private readonly AppDbContext _appDbContext;
        public ContractRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public async Task<Contract> GetContractById(string contractId)
        {
            var contract = await _appDbContext.Contracts.Where(x => x.Id == contractId)
                .Include(x => x.UserContracts)
                .ThenInclude (x => x.User)
                .Include(x => x.Project)
                .Include(x => x.ShareEquities)
                .Include(x => x.Disbursements)
                .FirstOrDefaultAsync();
            return contract;
        }
        public async Task<IEnumerable<Contract>> GetContractsByUserIdInAProject(string userId, string projectId, int pageIndex, int pageSize)
        {
            pageIndex = pageIndex < 1 ? 0 : pageIndex - 1;
            pageSize = pageSize < 1 ? 10 : pageSize;
            var contract = await _appDbContext.Contracts.Where(x => x.ProjectId.Equals(projectId) && x.UserContracts.Any(us => us.UserId.Equals(userId)))
                .Include(x => x.UserContracts)
                .ThenInclude(x => x.User)
                .Include(x => x.Project)
                .Skip(pageIndex * pageSize).Take(pageSize)
                .ToListAsync();
            return contract;
        }
        
    }
}
