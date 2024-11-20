using Microsoft.EntityFrameworkCore;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction, string>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Transaction> GetTransactionById(string transactionId)
        {
            var transaction = await _dbSet.Where(x=>x.Id.Equals(transactionId)).Include(x => x.Assets)
                .Include(x => x.Disbursement)
                .ThenInclude(x => x.Investor)
                .Include(x => x.Disbursement)
                .ThenInclude(x => x.Contract)
                .Include(x=>x.Finance)
                .FirstOrDefaultAsync();
            return transaction;
        }
        public IQueryable<Transaction> GetTransactionsListQuery(string projectId)
        {
            var query = _dbSet.Include(x => x.Assets).Where(x=>x.Finance.ProjectId.Equals(projectId))
                .Include(x => x.Disbursement)
                .ThenInclude(x => x.Investor)
                .Include(x => x.Disbursement)
                .ThenInclude(x => x.Contract)
                .Include(x => x.Finance);
            return query;
        }
    }
}
