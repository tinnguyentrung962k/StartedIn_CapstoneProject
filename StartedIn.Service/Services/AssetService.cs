using AutoMapper;
using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Services;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Asset;
using StartedIn.CrossCutting.DTOs.RequestDTO.Transaction;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Asset;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services
{
    public class AssetService : IAssetService
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProjectRepository _projectRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly UserManager<User> _userManager;
        private readonly IAzureBlobService _azureBlobService;
        private readonly ILogger<AssetService> _logger;
        private readonly IMapper _mapper;
        public AssetService(IAssetRepository assetRepository, 
            IUserService userService, 
            IUnitOfWork unitOfWork, 
            IProjectRepository projectRepository,
            UserManager<User> userManager,
            ITransactionRepository transactionRepository,
            IAzureBlobService azureBlobService,
            ILogger<AssetService> logger,
            IMapper mapper) 
        {
            _assetRepository = assetRepository;
            _userService = userService;
            _unitOfWork = unitOfWork;
            _projectRepository = projectRepository;
            _userManager = userManager;
            _transactionRepository = transactionRepository;
            _azureBlobService = azureBlobService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Asset> AddNewAssetToProject(string userId, string projectId, AssetCreateDTO assetCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }

            try
            {
                _unitOfWork.BeginTransaction();

                var project = await _projectRepository.GetProjectById(projectId);
                if (project.Finance == null)
                {
                    throw new InvalidOperationException("The project does not have a finance entity associated.");
                }

                var asset = new Asset
                {
                    AssetName = assetCreateDTO.AssetName,
                    CreatedBy = userInProject.User.FullName,
                    Price = assetCreateDTO.Price,
                    ProjectId = projectId,
                    PurchaseDate = assetCreateDTO.PurchaseDate,
                    Quantity = assetCreateDTO.Quantity,
                    Project = userInProject.Project,
                    SerialNumber = assetCreateDTO.SerialNumber,
                    Status = AssetStatus.Available,
                    RemainQuantity = assetCreateDTO.Quantity
                };
                var assetEntity = _assetRepository.Add(asset);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return assetEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while adding asset to project {projectId} by user {userId}: {ex}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<Asset> GetAssetDetailById(string userId, string projectId, string assetId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId); 
            var asset = await _assetRepository.GetAssetDetailById(assetId);
            if (asset == null) 
            {
                throw new NotFoundException(MessageConstant.AssetNotFound);
            }
            if (asset.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.AssetNotBelongToProject);
            }
            return asset;
        }
        public async Task<PaginationDTO<AssetResponseDTO>> FilterAssetInAProject(string userId, string projectId, int page, int size, AssetFilterDTO assetFilterDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var assetQuery = _assetRepository.GetAssetListQuery(projectId);
            if (!string.IsNullOrWhiteSpace(assetFilterDTO.SerialNumber))
            {
                assetQuery = assetQuery.Where(x => x.SerialNumber.Equals(assetFilterDTO.SerialNumber));
            }
            if (assetFilterDTO.Status != null)
            {
                assetQuery = assetQuery.Where(x => x.Status.Equals(assetFilterDTO.Status));
            }
            if (assetFilterDTO.FromDate.HasValue)
            {
                assetQuery = assetQuery.Where(x => x.PurchaseDate >= assetFilterDTO.FromDate.Value);
            }
            if (assetFilterDTO.ToDate.HasValue)
            {
                assetQuery = assetQuery.Where(x => x.PurchaseDate <= assetFilterDTO.ToDate.Value);
            }
            if (assetFilterDTO.FromPrice.HasValue)
            {
                assetQuery = assetQuery.Where(x => x.Price >= assetFilterDTO.FromPrice.Value);
            }
            if (assetFilterDTO.ToPrice.HasValue)
            {
                assetQuery = assetQuery.Where(x => x.Price <= assetFilterDTO.ToPrice.Value);
            }
            if (!string.IsNullOrWhiteSpace(assetFilterDTO.AssetName))
            {
                assetQuery = assetQuery.Where(x => x.AssetName.ToLower().Contains(assetFilterDTO.AssetName.ToLower()));
            }
            
            int totalCount = await assetQuery.CountAsync();

            var pagedResult = await assetQuery
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var response = _mapper.Map<List<AssetResponseDTO>>(pagedResult);
            var pagination = new PaginationDTO<AssetResponseDTO>
            {
                Total = totalCount,
                Size = size,
                Data = response,
                Page = page
            };
            return pagination;
        }
        public async Task DeleteAsset(string userId, string projectId, string assetId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var chosenAsset = await _assetRepository.GetOneAsync(assetId);
            if (chosenAsset == null)
            {
                throw new NotFoundException(MessageConstant.AssetNotFound);
            }
            if (chosenAsset.ProjectId != projectId) { 
                throw new UnmatchedException(MessageConstant.AssetNotBelongToProject);
            }
            if (chosenAsset.TransactionId != null)
            {
                throw new ExistedRecordException(MessageConstant.AssetBelongToTransaction + chosenAsset.TransactionId);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                await _assetRepository.SoftDeleteById(assetId);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Xoá thất bại", ex.Message);
                throw new Exception(ex.Message);
            }

        }
    }
}
