using Microsoft.EntityFrameworkCore;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities.BaseEntities;
using StartedIn.Repository.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories
{
    public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey>, IDisposable where TEntity : BaseAuditEntity<TKey>
    {
        protected internal readonly AppDbContext _context;
        protected internal readonly DbSet<TEntity> _dbSet;

        //Even though there is only one other repository, it is better to have a generic repository to avoid code duplication and extend codebase easily in the future.
        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public TEntity Add(TEntity entity)
        {
            _dbSet.Add(entity);
            return entity;
        }

        public async Task DeleteAsync(TEntity entity)
        {
            await Task.FromResult(_dbSet.Remove(entity));
        }

        public async Task SoftDeleteById(TKey id)
        {
            var entity = await GetOneAsync(id);
            entity.DeletedTime = DateTime.UtcNow;
            _dbSet.Update(entity);
        }

        public async Task DeleteByIdAsync(TKey id)
        {
            var entity = await GetOneAsync(id);
            _dbSet.Remove(entity);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<TEntity> GetOneAsync(TKey id)
        {
            return await _dbSet.FindAsync(id);
        }

        public IFluentRepository<TEntity> QueryHelper()
        {
            var repository = new FluentRepository<TEntity>(_dbSet);
            return repository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public TEntity Update(TEntity entity)
        {
            _dbSet.Update(entity);
            return entity;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<IEnumerable<TEntity>> GetPagingAsync(int pageIndex = 1, int pageSize = 1)
        {
            pageIndex = pageIndex < 1 ? 0 : pageIndex - 1;
            pageSize = pageSize < 1 ? 10 : pageSize;
            return await _dbSet.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

        }
        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public async virtual Task<IEnumerable<TEntity>> Get(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            int pageNum = 1,
            int pageSize = 5)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            string[] props = includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var includeProperty in props)
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

    }
}
