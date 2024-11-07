using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface IGenericRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        Task<TEntity> GetOneAsync(TKey id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> GetPagingAsync(int pageIndex = 1, int pageSize = 1);
        Task DeleteByIdAsync(TKey id);
        Task DeleteAsync(TEntity entity);
        TEntity Add(TEntity entity);
        TEntity Update(TEntity entity);
        Task<int> SaveChangesAsync();
        IFluentRepository<TEntity> QueryHelper();
        Task AddRangeAsync(IEnumerable<TEntity> entities);
    }
}
