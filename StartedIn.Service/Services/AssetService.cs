using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Asset;
using StartedIn.CrossCutting.DTOs.RequestDTO.Transaction;
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
        public AssetService(IAssetRepository assetRepository, 
            IUserService userService, 
            IUnitOfWork unitOfWork, 
            IProjectRepository projectRepository,
            UserManager<User> userManager,
            ITransactionRepository transactionRepository,
            IAzureBlobService azureBlobService,
            ILogger<AssetService> logger) 
        {
            _assetRepository = assetRepository;
            _userService = userService;
            _unitOfWork = unitOfWork;
            _projectRepository = projectRepository;
            _userManager = userManager;
            _transactionRepository = transactionRepository;
            _azureBlobService = azureBlobService;
            _logger = logger;
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
                    Status = AssetStatus.Available
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
    }
}
