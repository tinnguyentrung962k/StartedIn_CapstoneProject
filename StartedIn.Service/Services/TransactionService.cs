using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
        public TransactionService(
            ITransactionRepository transactionRepository, 
            IUserService userService, 
            UserManager<User> userManager,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IProjectRepository projectRepository,
            IAzureBlobService azureBlobService,
            ILogger<TransactionService> logger)
        {
             _transactionRepository = transactionRepository;
            _userService = userService;
            _userManager = userManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _projectRepository = projectRepository;
            _azureBlobService = azureBlobService;
            _logger = logger;
        }
        public async Task<PaginationDTO<TransactionResponseDTO>> GetListTransactionOfAProject(string userId, string projectId, int page, int size)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var transactions = _transactionRepository.QueryHelper()
                .Include(x => x.Finance)
                .Filter(x => x.Finance.ProjectId.Equals(projectId));
            var recordInPage = await transactions.GetPagingAsync(page, size);
            var response = _mapper.Map<List<TransactionResponseDTO>>(recordInPage);
            if (response.Any())
            {
                foreach (var transaction in response)
                {
                    var from = await _userService.GetUserWithId(recordInPage.First(t => t.Id == transaction.Id).FromID);
                    transaction.FromUserName = from.FullName;
                    var to = await _userService.GetUserWithId(recordInPage.First(t => t.Id == transaction.Id).ToID);
                    transaction.ToUserName = to.FullName;
                }
            }
            var pagination = new PaginationDTO<TransactionResponseDTO>
            {
                Data = response,
                Page = page,
                Size = size,
                Total = await transactions.GetTotal()

            };
            return pagination;
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
                var project = await _projectRepository.GetProjectById(projectId);
                _unitOfWork.BeginTransaction();
                var fileUrl = await _azureBlobService.UploadEvidenceOfTransaction(transactionCreateDTO.EvidenceFile);
                var transaction = new Transaction
                {
                    Amount = transactionCreateDTO.Amount,
                    Budget = transactionCreateDTO.Budget,
                    Content = transactionCreateDTO.Content,
                    CreatedBy = userInProject.User.FullName,
                    FinanceId = project.Finance.Id,
                    FromID = userInProject.User.Id,
                    ToID = transactionCreateDTO.ToInvestorID,
                    IsInFlow = transactionCreateDTO.IsInFlow,
                    Type = transactionCreateDTO.Type,
                    EvidenceUrl = fileUrl,
                };
                var transactionEntity = _transactionRepository.Add(transaction);
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
        public async Task<Transaction> GetTransactionDetailById(string userId, string projectId, string transactionId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var transaction = await _transactionRepository.GetTransactionById(transactionId);
            if (transaction == null)
            {
                throw new NotFoundException(MessageConstant.TransactionNotFound);
            }
            return transaction;
        }
    }
}
