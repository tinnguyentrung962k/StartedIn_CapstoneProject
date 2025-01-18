using AutoMapper;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Transaction;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Asset;
using StartedIn.CrossCutting.DTOs.ResponseDTO.DealOffer;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
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
        public static DateTimeOffset? ConvertToDateTimeOffset(DateOnly? dateOnly, bool isEndOfDay = false)
        {
            if (dateOnly.HasValue)
            {
                // Convert DateOnly to DateTime with either start or end of the day
                var dateTime = isEndOfDay
                    ? dateOnly.Value.ToDateTime(TimeOnly.MaxValue)  // End of the day (23:59:59)
                    : dateOnly.Value.ToDateTime(TimeOnly.MinValue); // Start of the day (00:00:00)

                return new DateTimeOffset(dateTime, TimeSpan.Zero);  // Adjust TimeSpan if needed based on your time zone
            }
            return null;
        }

        public async Task<PaginationDTO<TransactionResponseDTO>> GetListTransactionOfAProject(string userId, string projectId, TransactionFilterDTO transactionFilterDTO, int page, int size)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var transactions = _transactionRepository.GetTransactionsListQuery(projectId);
            if (transactionFilterDTO.DateFrom.HasValue)
            {
                var dateFrom = ConvertToDateTimeOffset(transactionFilterDTO.DateFrom, false);  // Start of the day
                transactions = transactions.Where(t => t.CreatedTime >= dateFrom);
            }

            // Convert DateTo to DateTimeOffset at the end of the day (23:59:59)
            if (transactionFilterDTO.DateTo.HasValue)
            {
                var dateTo = ConvertToDateTimeOffset(transactionFilterDTO.DateTo, true);  // End of the day
                transactions = transactions.Where(t => t.CreatedTime <= dateTo);
            }

            if (transactionFilterDTO.AmountFrom.HasValue)
            {
                transactions = transactions.Where(t => t.Amount >= transactionFilterDTO.AmountFrom.Value);
            }

            if (transactionFilterDTO.AmountTo.HasValue)
            {
                transactions = transactions.Where(t => t.Amount <= transactionFilterDTO.AmountTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(transactionFilterDTO.FromName))
            {
                transactions = transactions.Where(t => t.FromName.Contains(transactionFilterDTO.FromName));
            }

            if (!string.IsNullOrWhiteSpace(transactionFilterDTO.ToName))
            {
                transactions = transactions.Where(t => t.ToName.Contains(transactionFilterDTO.ToName));
            }

            if (transactionFilterDTO.IsInFlow != null)
            {
                transactions = transactions.Where(t => t.IsInFlow == transactionFilterDTO.IsInFlow);
            }

            if (transactionFilterDTO.Type != null) // Assuming you have an "All" option in your TransactionType enum
            {
                transactions = transactions.Where(t => t.Type == transactionFilterDTO.Type);
            }
            int totalCount = await transactions.CountAsync();
            var pagedResult = await transactions
                .Skip((page - 1) * size)
                .Take(size).ToListAsync();
            var response = _mapper.Map<List<TransactionResponseDTO>>(pagedResult);
            foreach (var transaction in response)
            {
                var fromUser = await _userManager.FindByIdAsync(transaction.FromID);
                if (fromUser != null)
                {
                    transaction.FromUserProfilePicture = fromUser.ProfilePicture;
                }
                var toUser = await _userManager.FindByIdAsync(transaction.ToID);
                if (toUser != null)
                {
                    transaction.ToUserProfilePicture = toUser.ProfilePicture;
                }
            }
            var pagination = new PaginationDTO<TransactionResponseDTO>
            {
                Data = response,
                Page = page,
                Size = size,
                Total = totalCount

            };
            return pagination;
        }

        public async Task<PaginationDTO<TransactionResponseDTO>> GetListTransactionOfUser(string userId, TransactionFilterDTO transactionFilterDTO, int page, int size)
        {
            var transactions = _transactionRepository.GetTransactionListQueryForUser(userId);

            // Filter by DateFrom (start of the day)
            if (transactionFilterDTO.DateFrom.HasValue)
            {
                var dateFrom = ConvertToDateTimeOffset(transactionFilterDTO.DateFrom, false);
                transactions = transactions.Where(t => t.CreatedTime >= dateFrom);
            }

            // Filter by DateTo (end of the day)
            if (transactionFilterDTO.DateTo.HasValue)
            {
                var dateTo = ConvertToDateTimeOffset(transactionFilterDTO.DateTo, true);
                transactions = transactions.Where(t => t.CreatedTime <= dateTo);
            }

            // Filter by Amount range
            if (transactionFilterDTO.AmountFrom.HasValue)
            {
                transactions = transactions.Where(t => t.Amount >= transactionFilterDTO.AmountFrom.Value);
            }

            if (transactionFilterDTO.AmountTo.HasValue)
            {
                transactions = transactions.Where(t => t.Amount <= transactionFilterDTO.AmountTo.Value);
            }

            // Filter by FromName
            if (!string.IsNullOrWhiteSpace(transactionFilterDTO.FromName))
            {
                transactions = transactions.Where(t => t.FromName.Contains(transactionFilterDTO.FromName));
            }

            // Filter by ToName
            if (!string.IsNullOrWhiteSpace(transactionFilterDTO.ToName))
            {
                transactions = transactions.Where(t => t.ToName.Contains(transactionFilterDTO.ToName));
            }

            // Filter by IsInFlow
            if (transactionFilterDTO.IsInFlow != null)
            {
                if (transactionFilterDTO.IsInFlow.Value)
                {
                    // Inflow: user is the receiver
                    transactions = transactions.Where(t => t.ToID == userId);
                }
                else
                {
                    // Outflow: user is the sender
                    transactions = transactions.Where(t => t.FromID == userId);
                }
            }

            // Filter by Transaction Type
            if (transactionFilterDTO.Type != null)
            {
                transactions = transactions.Where(t => t.Type == transactionFilterDTO.Type);
            }

            // Total count for pagination
            int totalCount = await transactions.CountAsync();

            // Fetch paged results
            var pagedResult = await transactions
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
            // Map to response DTO and dynamically set IsInFlow
            var response = pagedResult.Select(t => new TransactionResponseDTO
            {
                Id = t.Id,
                DisbursementId = t.DisbursementId,
                Amount = t.Amount.ToString(),
                FromID = t.FromID,
                ToID = t.ToID,
                FromUserName = t.FromName,
                ToUserName = t.ToName,
                Type = t.Type,
                Content = t.Content,
                EvidenceUrl = t.EvidenceUrl,
                Assets = _mapper.Map<List<AssetResponseDTO>>(t.Assets),
                Disbursement = _mapper.Map<DisbursementDetailInATransactionResponseDTO>(t.Disbursement),
                LastUpdatedTime = t.LastUpdatedTime,
                ProjectId = t.Finance.ProjectId, 
                IsInFlow = t.ToID == userId
            }).ToList();

            // Add additional user information (e.g., profile pictures)
            foreach (var transaction in response)
            {
                var fromUser = await _userManager.FindByIdAsync(transaction.FromID);
                if (fromUser != null)
                {
                    transaction.FromUserProfilePicture = fromUser.ProfilePicture;
                }
                var toUser = await _userManager.FindByIdAsync(transaction.ToID);
                if (toUser != null)
                {
                    transaction.ToUserProfilePicture = toUser.ProfilePicture;
                }
            }

            // Create and return pagination result
            var pagination = new PaginationDTO<TransactionResponseDTO>
            {
                Data = response,
                Page = page,
                Size = size,
                Total = totalCount
            };

            return pagination;
        }
        public async Task<TransactionInAndOutMoneyDTO> GetInAndOutMoneyTransactionOfCurrentMonth(string projectId) 
        {
            var now = DateTimeOffset.UtcNow; // Current time in UTC
            var startOfCurrentMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero); // Start of the current month
            var startOfNextMonth = startOfCurrentMonth.AddMonths(1); // Start of the next month

            // Fetch transactions for the given project ID
            var transactionsList = await _transactionRepository
                .GetTransactionsListQuery(projectId)
                .ToListAsync();

            // Filter transactions for the current month
            var transactionsCurrentMonth = transactionsList
                .Where(t => t.CreatedTime >= startOfCurrentMonth && t.CreatedTime < startOfNextMonth);

            // Calculate In and Out Money for the current month
            var totalInCurrentMonth = transactionsCurrentMonth
                .Where(t => t.IsInFlow is true)
                .Sum(t => t.Amount);

            var totalOutCurrentMonth = transactionsCurrentMonth
                .Where(t => t.IsInFlow is false)
                .Sum(t => t.Amount);

            // Return the DTO with the calculated values
            return new TransactionInAndOutMoneyDTO
            {
                InMoney = totalInCurrentMonth.ToString(),
                OutMoney = totalOutCurrentMonth.ToString()
            };
        }
        public async Task<Transaction> AddAnTransactionForProject(string userId, string projectId, TransactionCreateDTO transactionCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            if (userInProject.RoleInTeam != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var project = await _projectRepository.GetProjectById(projectId);
            if (project.Finance.CurrentBudget < transactionCreateDTO.Amount && transactionCreateDTO.IsInFlow == false)
            {
                throw new InvalidOperationException(MessageConstant.TransactionAmountOverProjectBudget);
            }
            if (transactionCreateDTO.Assets != null)
            {
                // Calculate the total price of all assets
                var totalAssetPrice = transactionCreateDTO.Assets
                    .Where(asset => asset.Price.HasValue && asset.Quantity.HasValue)
                    .Sum(asset => asset.Price.Value * asset.Quantity.Value);

                // Validate that the transaction amount matches the total asset price
                if (transactionCreateDTO.Amount != totalAssetPrice)
                {
                    throw new UnmatchedException(MessageConstant.TransactionAmountNotMatchTotalPurchaseAssets);
                }
                if (project.Finance.CurrentBudget < totalAssetPrice)
                {
                    throw new InvalidOperationException(MessageConstant.TransactionAmountOverProjectBudget);
                }
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var transaction = new Transaction
                {
                    Amount = transactionCreateDTO.Amount,
                    Content = transactionCreateDTO.Content,
                    CreatedBy = userInProject.User.FullName,
                    FinanceId = project.Finance.Id,
                    IsInFlow = transactionCreateDTO.IsInFlow,
                    Type = transactionCreateDTO.Type
                };
                if (!string.IsNullOrWhiteSpace(transactionCreateDTO.ToId))
                {
                    var toUser = await _userManager.FindByIdAsync(transactionCreateDTO.ToId);
                    transaction.ToID = transactionCreateDTO.ToId;
                    transaction.ToName = toUser.FullName;
                }
                if (!string.IsNullOrWhiteSpace(transactionCreateDTO.FromId))
                {
                    var fromUser = await _userManager.FindByIdAsync(transactionCreateDTO.FromId);
                    transaction.FromID = transactionCreateDTO.FromId;
                    transaction.FromName = fromUser.FullName;
                }
                if (string.IsNullOrWhiteSpace(transactionCreateDTO.FromId))
                {
                    transaction.FromName = transactionCreateDTO.FromName;
                }
                if (string.IsNullOrWhiteSpace(transactionCreateDTO.ToId))
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
                            PurchaseDate = DateOnly.FromDateTime(DateTime.Now),
                            Quantity = asset.Quantity,
                            Status = AssetStatus.Available,
                            SerialNumber = asset.SerialNumber,
                            Transaction = transactionEntity,
                            TransactionId = transactionEntity.Id,
                            RemainQuantity = asset.Quantity
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

        public async Task<TransactionResponseDTO> GetTransactionDetailById(string userId, string projectId, string transactionId)
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
            var transactionResponse = _mapper.Map<TransactionResponseDTO>(transaction);
            var fromUser = await _userManager.FindByIdAsync(transaction.FromID);
            if (fromUser != null)
            {
                transactionResponse.FromUserProfilePicture = fromUser.ProfilePicture;
            }
            var toUser = await _userManager.FindByIdAsync(transaction.ToID);
            if (toUser != null)
            {
                transactionResponse.ToUserProfilePicture = toUser.ProfilePicture;
            }
            return transactionResponse;
        }

        public async Task<TransactionResponseDTO> GetTransactionDetailByIdForSelfUser(string transactionId)
        {
            var transaction = await _transactionRepository.GetTransactionById(transactionId);
            if (transaction == null)
            {
                throw new NotFoundException(MessageConstant.TransactionNotFound);
            }
            var transactionResponse = _mapper.Map<TransactionResponseDTO>(transaction);
            var fromUser = await _userManager.FindByIdAsync(transaction.FromID);
            if (fromUser != null)
            {
                transactionResponse.FromUserProfilePicture = fromUser.ProfilePicture;
            }
            var toUser = await _userManager.FindByIdAsync(transaction.ToID);
            if (toUser != null)
            {
                transactionResponse.ToUserProfilePicture = toUser.ProfilePicture;
            }
            return transactionResponse;
        }
    }
}
