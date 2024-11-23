using AutoMapper;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Transaction;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.DealOffer;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Transaction;
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
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProjectRepository _projectRepository;
        private readonly IAzureBlobService _azureBlobService;
        private readonly ILogger<TransactionService> _logger;
        private readonly IAssetRepository _assetRepository;
        private readonly IFinanceRepository _financeRepository;
        public TransactionService(
            ITransactionRepository transactionRepository, 
            IUserService userService, 
            UserManager<User> userManager,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IProjectRepository projectRepository,
            IAzureBlobService azureBlobService,
            ILogger<TransactionService> logger,
            IAssetRepository assetRepository,
            IFinanceRepository financeRepository)
        {
             _transactionRepository = transactionRepository;
            _userService = userService;
            _userManager = userManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _projectRepository = projectRepository;
            _azureBlobService = azureBlobService;
            _logger = logger;
            _assetRepository = assetRepository;
            _financeRepository = financeRepository;
        }
        public async Task<PaginationDTO<TransactionResponseDTO>> GetListTransactionOfAProject(string userId, string projectId, int page, int size)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var transactions = _transactionRepository.GetTransactionsListQuery(projectId);
            int totalCount =  await transactions.CountAsync();
            var pagedResult = await transactions
                .Skip((page - 1) * size)
                .Take(size).ToListAsync();
            var response = _mapper.Map<List<TransactionResponseDTO>>(pagedResult);
            var pagination = new PaginationDTO<TransactionResponseDTO>
            {
                Data = response,
                Page = page,
                Size = size,
                Total = totalCount

            };
            return pagination;
        }
        public async Task<TransactionInAndOutMoneyDTO> GetInAndOutMoneyTransactionOfPreviousMonth(string projectId) 
        {
            var now = DateTimeOffset.UtcNow; // Lấy thời gian hiện tại theo UTC với kiểu DateTimeOffset
            var startOfCurrentMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero); // Ngày đầu tiên của tháng hiện tại
            var startOfPreviousMonth = startOfCurrentMonth.AddMonths(-1); // Ngày đầu tiên của tháng trước
            var endOfPreviousMonth = startOfCurrentMonth.AddTicks(-1); // Ngày cuối cùng của tháng trước

            // Fetch transactions for the given project ID
            var transactionsList = await _transactionRepository
                .GetTransactionsListQuery(projectId)
                .ToListAsync();

            // Filter transactions for the previous month
            var transactionsPreviousMonth = transactionsList
                .Where(t => t.CreatedTime >= startOfPreviousMonth && t.CreatedTime <= endOfPreviousMonth);

            // Calculate In and Out Money for the previous month
            var totalInPreviousMonth = transactionsPreviousMonth
                .Where(t => t.IsInFlow is true)
                .Sum(t => t.Amount);

            var totalOutPreviousMonth = transactionsPreviousMonth
                .Where(t => t.IsInFlow is false)
                .Sum(t => t.Amount);

            // Return the DTO with the calculated values
            return new TransactionInAndOutMoneyDTO
            {
                InMoney = totalInPreviousMonth.ToString(),
                OutMoney = totalOutPreviousMonth.ToString()
            };

        }
        public async Task<Transaction> AddAnTransactionForProject(string userId, string projectId, TransactionCreateDTO transactionCreateDTO)
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
                var transaction = new Transaction
                {
                    Amount = transactionCreateDTO.Amount,
                    Content = transactionCreateDTO.Content,
                    CreatedBy = userInProject.User.FullName,
                    FinanceId = project.Finance.Id,
                    IsInFlow = transactionCreateDTO.IsInFlow,
                    Type = transactionCreateDTO.Type
                };
                if (transactionCreateDTO.ToId != null)
                {
                    var toUser = await _userManager.FindByIdAsync(transactionCreateDTO.ToId);
                    transaction.ToID = transactionCreateDTO.ToId;
                    transaction.ToName = toUser.FullName;
                }
                if (transactionCreateDTO.FromId != null)
                {
                    var fromUser = await _userManager.FindByIdAsync(transactionCreateDTO.FromId);
                    transaction.FromID = transactionCreateDTO.FromId;
                    transaction.ToName = fromUser.FullName;
                }
                if (transactionCreateDTO.FromId == null)
                {
                    transaction.FromName = transactionCreateDTO.FromName;
                }
                if (transactionCreateDTO.ToId == null)
                {
                    transaction.ToName = transactionCreateDTO.ToName;
                }
                var transactionEntity = _transactionRepository.Add(transaction);
                if (transactionEntity.IsInFlow == true)
                {
                    project.Finance.CurrentBudget += transactionEntity.Amount;
                }
                if (transactionEntity.IsInFlow == false)
                {
                    project.Finance.CurrentBudget -= transactionEntity.Amount;
                    project.Finance.TotalExpense += transactionEntity.Amount;
                }
                var finance = _financeRepository.Update(project.Finance);
                if (transactionCreateDTO.Assets != null)
                {
                    List<Asset> assets = new List<Asset>();
                    foreach (var asset in transactionCreateDTO.Assets)
                    {
                        Asset newAsset = new Asset
                        {
                            AssetName = asset.AssetName,
                            CreatedBy = userInProject.User.FullName,
                            Price = asset.Price,
                            Project = userInProject.Project,
                            ProjectId = projectId,
                            PurchaseDate = asset.PurchaseDate,
                            Quantity = asset.Quantity,
                            Status = AssetStatus.Available,
                            SerialNumber = asset.SerialNumber,
                            Transaction = transactionEntity,
                            TransactionId = transactionEntity.Id
                        };
                        assets.Add(newAsset);
                    }
                    await _assetRepository.AddRangeAsync(assets); 
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return transactionEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while creating the transaction: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }    
        }
        public async Task<Transaction> UploadEvidenceFile(string userId, string projectId, string transactionId, IFormFile file)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var transaction = await _transactionRepository.GetTransactionById(transactionId);
            if (transaction == null)
            {
                throw new NotFoundException(MessageConstant.TransactionNotFound);
            }
            if (transaction.Finance.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.TransactionNotBelongToProject);
            }
            try
            {
                var fileUrl = await _azureBlobService.UploadEvidenceOfTransaction(file);
                transaction.EvidenceUrl = fileUrl;
                var updatedTransaction = _transactionRepository.Update(transaction);
                await _unitOfWork.SaveChangesAsync();
                return updatedTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while update the transaction: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<Transaction> GetTransactionDetailById(string userId, string projectId, string transactionId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var transaction = await _transactionRepository.GetTransactionById(transactionId);
            if (transaction == null)
            {
                throw new NotFoundException(MessageConstant.TransactionNotFound);
            }
            if (transaction.Finance.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.TransactionNotBelongToProject);
            }
            return transaction;
        }
    }
}
