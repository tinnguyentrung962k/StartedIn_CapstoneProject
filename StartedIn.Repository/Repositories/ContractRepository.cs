using Microsoft.EntityFrameworkCore;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using System;
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
                .ThenInclude(x=>x.User)
                .Include(x => x.Disbursements)
                .Include(x => x.DealOffer)
                .ThenInclude(x => x.InvestmentCall)
                .Where(x=>x.DeletedTime == null)
                .FirstOrDefaultAsync();
            return contract;
        }
        public async Task<List<Contract>> GetContractByProjectId(string projectId)
        {
            var contract = await _appDbContext.Contracts.Where(x => x.ProjectId == projectId)
                .Include(x => x.UserContracts)
                .ThenInclude(x => x.User)
                .Include(x => x.Project)
                .Include(x => x.ShareEquities)
                .Include(x => x.Disbursements)
                .Include(x => x.DealOffer)
                .Where(x => x.DeletedTime == null)
                .ToListAsync();
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
                .Where(x => x.DeletedTime == null)
                .Skip(pageIndex * pageSize).Take(pageSize)
                .ToListAsync();
            return contract;
        }
        public async Task<Contract> GetTheNearestExpiredInternalContractInAProject(string projectId)
        {
             var nearestExpiredContract = await _appDbContext.Contracts
               .Include(x => x.UserContracts)
               .ThenInclude(x => x.User)
               .Include(x => x.Project)
               .Include(x => x.ShareEquities)
               .Include(x => x.Disbursements)
               .Include(x => x.DealOffer)
               .Where(x => x.ProjectId.Equals(projectId) && x.ContractType.Equals(ContractTypeEnum.INTERNAL))
               .Where(x => x.ExpiredDate < DateOnly.FromDateTime(DateTime.UtcNow))
               .OrderByDescending(x => x.ExpiredDate)
               .Where(x => x.DeletedTime == null)
               .FirstOrDefaultAsync();
            return nearestExpiredContract;
        }
        public IQueryable<Contract> GetContractListQuery(string userId, string projectId)
        {
            var query = _appDbContext.Contracts
                .Include(x => x.Disbursements)
                .Include(x => x.UserContracts)
                .Where(x => x.ProjectId.Equals(projectId) 
                && x.UserContracts.Any(us => us.UserId.Equals(userId) || us.TransferToId.Equals(userId))
                && x.DeletedTime == null)
                .OrderByDescending(x => x.LastUpdatedTime);
            return query;
        }
        public async Task UpdateUserInContract(UserContract userContract)
        {
            var trackedEntity = _appDbContext.Set<UserContract>().Local
            .FirstOrDefault(up => up.UserId == userContract.UserId && up.ContractId == userContract.ContractId);

            // Detach the tracked entity if found
            if (trackedEntity != null)
            {
                _appDbContext.Entry(trackedEntity).State = EntityState.Detached;
            }

            // Update the entity
            _appDbContext.Set<UserContract>().Update(userContract);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
