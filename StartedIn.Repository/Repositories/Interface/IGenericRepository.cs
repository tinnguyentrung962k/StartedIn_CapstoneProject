using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface IGenericRepository<TEntity, TKey> where TEntity : BaseAuditEntity<TKey>
    {
        Task<TEntity> GetOneAsync(TKey id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> GetPagingAsync(int pageIndex = 1, int pageSize = 1);
        Task DeleteByIdAsync(TKey id);
        Task DeleteAsync(TEntity entity);
        TEntity Add(TEntity entity);
        TEntity Update(TEntity entity);
        Task<int> SaveChangesAsync();
        Task SoftDeleteById(TKey id);
        IFluentRepository<TEntity> QueryHelper();
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        Task<IEnumerable<TEntity>> Get(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            int pageNum = 1,
            int pageSize = 5);
    }
}
