using StartedIn.CrossCutting.DTOs.RequestDTO.Asset;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Asset;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
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
        Task<PaginationDTO<AssetResponseDTO>> FilterAssetInAProject(string userId, string projectId, int page, int size, AssetFilterDTO assetFilterDTO);
        Task DeleteAsset(string userId, string projectId, string assetId);
        Task<Asset> UpdateAsset(string userId, string projectId, string assetId, AssetUpdateDTO assetUpdateDTO);
    }
}
