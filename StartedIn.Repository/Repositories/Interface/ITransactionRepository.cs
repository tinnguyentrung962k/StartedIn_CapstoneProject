﻿using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface ITransactionRepository : IGenericRepository<Transaction,string>
    {
        Task<Transaction> GetTransactionById(string transactionId);
        IQueryable<Transaction> GetTransactionsListQuery(string projectId);
        IQueryable<Transaction> GetTransactionListQueryForUser(string userId);
    }
}
