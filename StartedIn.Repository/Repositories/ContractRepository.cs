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
                .FirstOrDefaultAsync();
            return contract;
        }
    }
}
