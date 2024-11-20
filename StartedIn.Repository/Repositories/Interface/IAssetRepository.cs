using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface IAssetRepository :  IGenericRepository<Asset, string>
    {
        Task<Asset> GetAssetDetailById(string assetId);
    }
}
