using StartedIn.CrossCutting.DTOs.RequestDTO.Asset;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IAssetService
    {
        Task<Asset> GetAssetDetailById(string userId, string projectId, string assetId);
        Task<Asset> AddNewAssetToProject(string userId, string projectId, AssetCreateDTO assetCreateDTO);
    }
}
