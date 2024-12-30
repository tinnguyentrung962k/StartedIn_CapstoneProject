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
    public class AssetRepository : GenericRepository<Asset, string>, IAssetRepository
    {
        public AssetRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Asset> GetAssetDetailById(string assetId)
        {
            var asset = await _dbSet.Where(x => x.Id.Equals(assetId))
                .Include(x=>x.Transaction)
                .FirstOrDefaultAsync();
            return asset;
        }
        public IQueryable<Asset> GetAssetListQuery(string projectId)
        {
            var query = _dbSet.Where(x => x.ProjectId.Equals(projectId) && x.DeletedTime == null);
            return query;
                
        }
    }
}
