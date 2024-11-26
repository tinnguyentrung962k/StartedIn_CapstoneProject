using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Services;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.RequestDTO.Tasks;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Contract;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services
{
    public class DisbursementService : IDisbursementService
    {
        private readonly IDisbursementRepository _disbursementRepository;
        private IUnitOfWork _unitOfWork;
        private readonly ILogger<Disbursement> _logger;
        private readonly IProjectRepository _projectRepository;
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IFinanceRepository _financeRepository;
        private readonly IAzureBlobService _azureBlobService;
        public DisbursementService(
            IDisbursementRepository disbursementRepository, 
            IUnitOfWork unitOfWork, 
            ILogger<Disbursement> logger, 
            IProjectRepository projectRepository, 
            IEmailService emailService, 
            UserManager<User> userManager, 
            IUserService userService, 
            IMapper mapper,
            ITransactionRepository transactionRepository,
            IFinanceRepository financeRepository,
            IAzureBlobService azureBlobService)
        {
            _disbursementRepository = disbursementRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _projectRepository = projectRepository;
            _emailService = emailService;
            _userManager = userManager;
            _userService = userService;
            _mapper = mapper;
            _transactionRepository = transactionRepository;
            _financeRepository = financeRepository;
            _azureBlobService = azureBlobService;
        }
        public async Task FinishedTheTransaction(string disbursementId, string apiKey)
        {
            var disbursement = await _disbursementRepository.GetDisbursementById(disbursementId);
            if (disbursement is null)
            {
                throw new NotFoundException(MessageConstant.DisbursementNotFound);
            }
            var project = await _projectRepository.GetProjectAndMemberByProjectId(disbursement.Contract.ProjectId);
            if (project == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }
            // Check if the disbursement is already finished to avoid reprocessing
            if (disbursement.DisbursementStatus == CrossCutting.Enum.DisbursementStatusEnum.FINISHED)
            {
                _logger.LogWarning($"Disbursement {disbursementId} is already finished. Skipping transaction.");
                return; // Skip processing as it's already finished
            }

            if (disbursement.Contract.ProjectId != project.Id)
            {
                throw new UnmatchedException(MessageConstant.DisbursementNotBelongToProject);
            }

            if (apiKey != DecryptString(project.HarshPayOsApiKey))
            {
                throw new UnauthorizedAccessException(MessageConstant.RolePermissionError);
            }
            try
            {
                await UpdateFinanceAndTransactionOfProjectOfFinishedDisbursement(disbursement.Investor,project.Id, disbursement);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while updating a disbursement: {ex.Message}");
                disbursement.DisbursementStatus = CrossCutting.Enum.DisbursementStatusEnum.ERROR;
                _disbursementRepository.Update(disbursement);
                await _unitOfWork.SaveChangesAsync(); // Update to failed status in case of error
                await _unitOfWork.RollbackAsync(); // Rollback transaction
                throw; // Re-throw the exception after rollback
            }
        }
        public async Task CancelPayment(string disbursementId, string apiKey)
        {
            var disbursement = await _disbursementRepository.GetDisbursementById(disbursementId);
            if (disbursement is null)
            {
                throw new NotFoundException(MessageConstant.DisbursementNotFound);
            }
            var project = await _projectRepository.GetProjectById(disbursement.Contract.ProjectId);
            if (project == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }
            if (apiKey != DecryptString(project.HarshPayOsApiKey))
            {
                throw new UnauthorizedAccessException(MessageConstant.RolePermissionError);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                _logger.LogInformation($"Payment for Disbursement {disbursementId} under Project {project.Id} has been requested to be canceled.");
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing payment cancellation: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        private string DecryptString(string cipherText)
        {
            var key = Encoding.UTF8.GetBytes("1234567890abcdef");
            var iv = Encoding.UTF8.GetBytes("abcdef0123456789");

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public async Task ReminderDisbursementForInvestor()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            int reminderDaysBeforeEndDate = 2;

            // Lọc các khoản giải ngân có EndDate trong vòng 2 ngày từ hôm nay
            var upcomingDisbursements = await _disbursementRepository.QueryHelper()
                .Filter(x => x.EndDate == today.AddDays(reminderDaysBeforeEndDate) && x.IsValidWithContract == true) // Lọc ngày kết thúc chính xác
                .Include(x => x.Investor)
                .Include(x => x.Contract)
                .GetAllAsync();

            foreach (var upcomingDisbursement in upcomingDisbursements)
            {
                // Tìm nhà đầu tư và dự án
                var investor = await _userManager.FindByIdAsync(upcomingDisbursement.InvestorId);
                var project = await _projectRepository.GetProjectById(upcomingDisbursement.Contract.ProjectId);

                // Kiểm tra nếu nhà đầu tư và dự án tồn tại
                if (investor != null && project != null)
                {
                    try
                    {
                        // Gửi email nhắc nhở cho nhà đầu tư
                        await _emailService.SendDisbursementReminder(
                            investor.Email,
                            upcomingDisbursement.EndDate,
                            project.ProjectName,
                            upcomingDisbursement.Title,
                            upcomingDisbursement.Amount
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error sending reminder email to {investor.Email} for disbursement {upcomingDisbursement.Title}: {ex.Message}");
                    }
                }
                else
                {
                    _logger.LogWarning($"Investor or Project not found for Disbursement ID: {upcomingDisbursement.Id}");
                }
            }
        }
        public async Task AutoUpdateOverdueIfDisbursementsExpire()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);

            // Retrieve all expired disbursements that need updating
            var expiredDisbursements = await _disbursementRepository.QueryHelper()
                .Filter(x => x.EndDate < today && x.IsValidWithContract == true && x.DisbursementStatus == CrossCutting.Enum.DisbursementStatusEnum.PENDING)
                .Include(x => x.Investor)
                .Include(x => x.Contract)
                .GetAllAsync();
            foreach (var expiredDisbursement in expiredDisbursements)
            {
                expiredDisbursement.DisbursementStatus = CrossCutting.Enum.DisbursementStatusEnum.OVERDUE;
                _disbursementRepository.Update(expiredDisbursement);
            }
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task AcceptDisbursement(string userId, string disbursementId, List<IFormFile> files)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var disbursement = await _disbursementRepository.GetDisbursementById(disbursementId);
            if (disbursement == null)
            {
                throw new NotFoundException(MessageConstant.DisbursementNotFound);
            }
            if (disbursement.InvestorId != userId)
            {
                throw new UnmatchedException(MessageConstant.DisbursementNotBelongToInvestor);
            }
            if (files == null || !files.Any())
            {
                throw new UploadFileException(MessageConstant.EmptyFileError);
            }

            try
            {
                _unitOfWork.BeginTransaction();

                // Upload files and store URLs with original filenames
                var fileUrls = await _azureBlobService.UploadEvidencesOfDisbursement(files);
                disbursement.DisbursementAttachments = files.Select((file, index) => new DisbursementAttachment
                {
                    EvidenceFile = fileUrls[index],
                    FileName = Path.GetFileName(file.FileName)
                }).ToList();

                disbursement.DisbursementStatus = CrossCutting.Enum.DisbursementStatusEnum.ACCEPTED;

                _disbursementRepository.Update(disbursement);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating a disbursement");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<PaginationDTO<DisbursementForLeaderInProjectResponseDTO>> GetDisbursementListForLeaderInAProject(string userId, string projectId, DisbursementFilterInProjectDTO disbursementFilterDTO, int size, int page)
        {
            var loginUser = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var filterDisbursements = _disbursementRepository.GetDisbursementListOfAProjectQuery(projectId);
            if (!string.IsNullOrWhiteSpace(disbursementFilterDTO.Title))
            {
                filterDisbursements = filterDisbursements.Where(d => d.Title != null && d.Title.ToLower().Contains(disbursementFilterDTO.Title.ToLower()));
            }
            if (disbursementFilterDTO.PeriodFrom.HasValue && disbursementFilterDTO.PeriodTo.HasValue)
            {
                filterDisbursements = filterDisbursements.Where(d => d.StartDate >= disbursementFilterDTO.PeriodFrom && d.EndDate <= disbursementFilterDTO.PeriodTo);
            }
            else if (disbursementFilterDTO.PeriodFrom.HasValue)
            {
                filterDisbursements = filterDisbursements.Where(d => d.StartDate >= disbursementFilterDTO.PeriodFrom);
            }
            else if (disbursementFilterDTO.PeriodTo.HasValue)
            {
                filterDisbursements = filterDisbursements.Where(d => d.EndDate <= disbursementFilterDTO.PeriodTo);
            }

            // Amount range filter
            if (disbursementFilterDTO.AmountFrom.HasValue)
            {
                filterDisbursements = filterDisbursements.Where(d => d.Amount >= disbursementFilterDTO.AmountFrom);
            }
            if (disbursementFilterDTO.AmountTo.HasValue)
            {
                filterDisbursements = filterDisbursements.Where(d => d.Amount <= disbursementFilterDTO.AmountTo);
            }

            // Status filter
            if (disbursementFilterDTO.DisbursementStatus.HasValue)
            {
                filterDisbursements = filterDisbursements.Where(d => d.DisbursementStatus == disbursementFilterDTO.DisbursementStatus);
            }

            // Investor filter
            if (!string.IsNullOrEmpty(disbursementFilterDTO.InvestorId))
            {
                filterDisbursements = filterDisbursements.Where(d => d.InvestorId.Equals(disbursementFilterDTO.InvestorId));
            }

            // Contract filter
            if (!string.IsNullOrWhiteSpace(disbursementFilterDTO.ContractId))
            {
                filterDisbursements = filterDisbursements.Where(d => d.ContractId.Equals(disbursementFilterDTO.ContractId));
            }

            int totalCount = await filterDisbursements.CountAsync();

            var pagedResult = await filterDisbursements
                .Skip((page - 1) * size)
                .Take(size)
                .Include(c => c.Contract)
                .Include(d => d.Investor)
                .ToListAsync();

            var disbursementResponseDTOs = pagedResult.Select(disbursement => new DisbursementForLeaderInProjectResponseDTO
            {
                Id = disbursement.Id,
                Amount = disbursement.Amount.ToString(),
                ContractIdNumber = disbursement.Contract.ContractIdNumber,
                DisbursementStatus = disbursement.DisbursementStatus,
                EndDate = disbursement.EndDate,
                StartDate = disbursement.StartDate,
                Title = disbursement.Title,
                InvestorName = disbursement.Investor.FullName
            }).ToList();
            var pagination = new PaginationDTO<DisbursementForLeaderInProjectResponseDTO>()
            {
                Data = _mapper.Map<IEnumerable<DisbursementForLeaderInProjectResponseDTO>>(disbursementResponseDTOs),
                Total = totalCount,
                Page = page,
                Size = size
            };

            return pagination;
        }

        public async Task<PaginationDTO<DisbursementForInvestorInInvestorMenuResponseDTO>> GetDisbursementListForInvestorInMenu(string userId, DisbursementFilterInvestorMenuDTO disbursementFilterDTO, int size, int page)
        {
            var investor = await _userManager.FindByIdAsync(userId);
            if (investor == null) 
            {
                throw new NotFoundException(MessageConstant.NotFoundUserError);
            }
            var filterDisbursements = _disbursementRepository.GetDisbursementListOfInvestorQuery(userId);
            if (!string.IsNullOrWhiteSpace(disbursementFilterDTO.Title))
            {
                filterDisbursements = filterDisbursements.Where(d => d.Title != null && d.Title.ToLower().Contains(disbursementFilterDTO.Title.ToLower()) && d.IsValidWithContract == true);
            }
            if (disbursementFilterDTO.PeriodFrom.HasValue && disbursementFilterDTO.PeriodTo.HasValue)
            {
                filterDisbursements = filterDisbursements.Where(d => d.StartDate >= disbursementFilterDTO.PeriodFrom && d.EndDate <= disbursementFilterDTO.PeriodTo && d.IsValidWithContract == true);
            }
            else if (disbursementFilterDTO.PeriodFrom.HasValue)
            {
                filterDisbursements = filterDisbursements.Where(d => d.StartDate >= disbursementFilterDTO.PeriodFrom);
            }
            else if (disbursementFilterDTO.PeriodTo.HasValue)
            {
                filterDisbursements = filterDisbursements.Where(d => d.EndDate <= disbursementFilterDTO.PeriodTo);
            }

            // Amount range filter
            if (disbursementFilterDTO.AmountFrom.HasValue)
            {
                filterDisbursements = filterDisbursements.Where(d => d.Amount >= disbursementFilterDTO.AmountFrom);
            }
            if (disbursementFilterDTO.AmountTo.HasValue)
            {
                filterDisbursements = filterDisbursements.Where(d => d.Amount <= disbursementFilterDTO.AmountTo);
            }

            // Status filter
            if (disbursementFilterDTO.DisbursementStatus.HasValue)
            {
                filterDisbursements = filterDisbursements.Where(d => d.DisbursementStatus == disbursementFilterDTO.DisbursementStatus);
            }
            // Project filter
            if (!string.IsNullOrEmpty(disbursementFilterDTO.ProjectId))
            {
                filterDisbursements = filterDisbursements.Where(d => d.Contract.ProjectId.Equals(disbursementFilterDTO.ProjectId));
            }
            // Contract filter
            if (!string.IsNullOrWhiteSpace(disbursementFilterDTO.ContractId))
            {
                filterDisbursements = filterDisbursements.Where(d => d.ContractId.Equals(disbursementFilterDTO.ContractId));
            }

            int totalCount = await filterDisbursements.CountAsync();

            var pagedResult = await filterDisbursements
                .Skip((page - 1) * size)
                .Take(size)
                .Include(c=>c.Contract)
                .ThenInclude(c=>c.Project)
                .Include(d=>d.Investor)
                .ToListAsync();

            var disbursementResponseDTOs = pagedResult.Select(disbursement => new DisbursementForInvestorInInvestorMenuResponseDTO
            {
                Id = disbursement.Id,
                Amount = disbursement.Amount.ToString(),
                ContractIdNumber = disbursement.Contract.ContractIdNumber,
                DisbursementStatus = disbursement.DisbursementStatus,
                EndDate = disbursement.EndDate,
                LogoUrl = disbursement.Contract.Project.LogoUrl,
                ProjectName = disbursement.Contract.Project.ProjectName,
                StartDate = disbursement.StartDate,
                Title = disbursement.Title

            }).ToList();

            var pagination = new PaginationDTO<DisbursementForInvestorInInvestorMenuResponseDTO>()
            {
                Data = _mapper.Map<IEnumerable<DisbursementForInvestorInInvestorMenuResponseDTO>>(disbursementResponseDTOs),
                Total = totalCount,
                Page = page,
                Size = size
            };

            return pagination;
        }

        public async Task<SelfDisbursmentOfInvestorDTO> GetSelfDisbursementForInvestor(string userId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var disbursementsOfAnInvestor = await _disbursementRepository.GetDisbursementListOfAnInvestorInAProjectQuery(userId, projectId).ToListAsync();
            var totalDisbursedAmount = disbursementsOfAnInvestor
                .Where(d => d.DisbursementStatus == DisbursementStatusEnum.FINISHED) // Lọc các đợt đã giải ngân
                .Sum(d => d.Amount); // Tổng số tiền đã giải ngân

            var totalRemainingDisbursement = disbursementsOfAnInvestor
                .Where(d => d.DisbursementStatus != DisbursementStatusEnum.FINISHED && d.DisbursementStatus != DisbursementStatusEnum.REJECTED) // Lọc các đợt chưa giải ngân
                .Sum(d => d.Amount); // Tổng số tiền chưa giải ngân

            // Tạo DTO trả về
            return new SelfDisbursmentOfInvestorDTO
            {
                SelfDisbursedAmount = totalDisbursedAmount.ToString(), // Chuyển đổi thành chuỗi
                SelfRemainingDisbursement = totalRemainingDisbursement.ToString() // Chuyển đổi thành chuỗi
            };
        }

        public async Task RejectADisbursement(string userId, string disbursementId, DisbursementRejectDTO disbursementRejectDTO)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var disbursement = await _disbursementRepository.GetDisbursementById(disbursementId);
            if (disbursement == null)
            {
                throw new NotFoundException(MessageConstant.DisbursementNotFound);
            }
            if (disbursement.InvestorId != user.Id)
            {
                throw new UnmatchedException(MessageConstant.DisbursementNotBelongToInvestor);
            }
            if (disbursement.DisbursementStatus == CrossCutting.Enum.DisbursementStatusEnum.ACCEPTED || disbursement.DisbursementStatus == CrossCutting.Enum.DisbursementStatusEnum.FINISHED)
            {
                throw new UpdateException(MessageConstant.CannotRejectThisDisbursement);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                disbursement.DisbursementStatus = CrossCutting.Enum.DisbursementStatusEnum.REJECTED;
                disbursement.DeclineReason = disbursementRejectDTO.DeclineReason;
                disbursement.LastUpdatedTime = DateTimeOffset.UtcNow;
                disbursement.LastUpdatedBy = user.FullName;
                _disbursementRepository.Update(disbursement);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError($"An error occurred while updating a disbursement: {ex.Message}");
                await _unitOfWork.RollbackAsync(); // Rollback transaction
                throw;
            }
        }
        public async Task DisbursementConfirmation(string userId, string projectId, string disbursementId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var disbursement = await _disbursementRepository.GetDisbursementById(disbursementId);
            if (disbursement == null)
            {
                throw new NotFoundException(MessageConstant.DisbursementNotFound);
            }
            if (disbursement.DisbursementStatus != CrossCutting.Enum.DisbursementStatusEnum.ACCEPTED)
            {
                throw new UpdateException(MessageConstant.UpdateFailed); 
            }
            if (userInProject.ProjectId != disbursement.Contract.ProjectId)
            {
                throw new UnmatchedException(MessageConstant.DisbursementNotBelongToProject);
            }
            try 
            {
                await UpdateFinanceAndTransactionOfProjectOfFinishedDisbursement(userInProject.User,projectId, disbursement);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while updating a disbursement: {ex.Message}");
                await _unitOfWork.RollbackAsync(); // Rollback transaction
                throw;
            }

        }
        public async Task UpdateFinanceAndTransactionOfProjectOfFinishedDisbursement(User user, string projectId, Disbursement disbursement)
        {
            _unitOfWork.BeginTransaction();
            var projectFinance = await _financeRepository.QueryHelper()
                    .Filter(x => x.ProjectId.Equals(projectId))
                    .GetOneAsync();
            var project = await _projectRepository.GetProjectAndMemberByProjectId(projectId);
            projectFinance.CurrentBudget += disbursement.Amount;
            projectFinance.DisbursedAmount += disbursement.Amount;
            projectFinance.RemainingDisbursement -= disbursement.Amount;
            projectFinance.LastUpdatedTime = DateTime.UtcNow;
            var investor = await _userManager.FindByIdAsync(disbursement.InvestorId);
            var leader = project.UserProjects.FirstOrDefault(x => x.ProjectId.Equals(project.Id) && x.RoleInTeam == CrossCutting.Enum.RoleInTeam.Leader).User;
            var newTransaction = new Transaction
            {
                Content = disbursement.Investor.FullName + " giải ngân: " + disbursement.Title,
                Amount = disbursement.Amount,
                Disbursement = disbursement,
                DisbursementId = disbursement.Id,
                FinanceId = projectFinance.Id,
                Type = CrossCutting.Enum.TransactionType.Disbursement,
                IsInFlow = true,
                FromID = disbursement.InvestorId,
                ToID = leader.Id,
                FromName = investor.FullName,
                ToName = leader.FullName
            };
            disbursement.DisbursementStatus = CrossCutting.Enum.DisbursementStatusEnum.FINISHED;
            disbursement.LastUpdatedBy = user.FullName;
            _disbursementRepository.Update(disbursement);
            _financeRepository.Update(projectFinance);
            _transactionRepository.Add(newTransaction);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }

        public async Task<Disbursement> GetADisbursementDetailForLeader(string userId, string projectId, string disbursementId)
        {
            var loginUser = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var disbursement = await _disbursementRepository.GetDisbursementById(disbursementId);
            if (disbursement == null)
            {
                throw new NotFoundException(MessageConstant.DisbursementNotFound);
            }
            if (disbursement.Contract.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.DisbursementNotBelongToProject);
            }
            return disbursement;
        }

        public async Task<Disbursement> GetADisbursementDetailInvestor(string userId, string disbursementId)
        {
            var investor = await _userManager.FindByIdAsync(userId);
            if (investor == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundUserError);
            }
            var disbursement = await _disbursementRepository.GetDisbursementById(disbursementId);
            if (disbursement == null)
            {
                throw new NotFoundException(MessageConstant.DisbursementNotFound);
            }
            if (disbursement.InvestorId != investor.Id)
            {
                throw new UnmatchedException(MessageConstant.DisbursementNotBelongToInvestor);
            }
            return disbursement;

        }
        public async Task<Disbursement> GetADisbursementDetailInProject(string userId, string projectId, string disbursementId)
        {
            var loginUser = await _userService.CheckIfUserInProject(userId, projectId);
            var disbursement = await _disbursementRepository.GetDisbursementById(disbursementId);
            if (disbursement == null)
            {
                throw new NotFoundException(MessageConstant.DisbursementNotFound);
            }
            if (disbursement.Contract.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.DisbursementNotBelongToProject);
            }
            return disbursement;
        }

        public async Task<DisbursementOverviewOfProject> GetADisbursementTotalInAMonth(string projectId, DateTime dateTime)
        {
            var disbursementOverview = await GetADisbursementOverviewInMonth(projectId, dateTime);
            return disbursementOverview;
        }
        public async Task<DisbursementOverviewOfProject> GetADisbursementOverviewInMonth(string projectId, DateTime dateTime)
        {
            var startOfMonth = new DateOnly(dateTime.Year, dateTime.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var doneDisbursements = await _disbursementRepository.GetDisbursementListOfAProjectQuery(projectId)
                    .Where(x => x.EndDate <= endOfMonth && x.EndDate >= startOfMonth && x.DisbursementStatus == DisbursementStatusEnum.FINISHED)
                    .ToListAsync();
            var remainingDisbursements = await _disbursementRepository.GetDisbursementListOfAProjectQuery(projectId)
                .Where(x => x.EndDate <= endOfMonth
                && x.EndDate >= startOfMonth
                && x.DisbursementStatus != DisbursementStatusEnum.FINISHED
                && x.DisbursementStatus != DisbursementStatusEnum.REJECTED).ToListAsync();

            decimal totalDisbursedAmount = doneDisbursements.Sum(d => d.Amount);
            decimal totalRemainingDisbursement = remainingDisbursements.Sum(d => d.Amount);

            return new DisbursementOverviewOfProject
            {
                DisbursedAmount = totalDisbursedAmount.ToString(),
                RemainingDisbursement = totalRemainingDisbursement.ToString(),
            };
        }

        public async Task<PaginationDTO<DisbursementOverviewOfProjectForInvestor>> GetADisbursementOverviewForInvestor(string userId, int page, int size)
        {
            // Fetch projects where the user is an investor
            var projectQuery = _projectRepository.GetProjectListQuery()
                .Where(x => x.UserProjects.Any(up => up.UserId.Equals(userId) && up.RoleInTeam.Equals(RoleInTeam.Investor)));

            // Total record count for pagination
            var totalRecords = await projectQuery.CountAsync();

            // Paginate the project list
            var projectList = await projectQuery
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var result = new List<DisbursementOverviewOfProjectForInvestor>();

            foreach (var project in projectList)
            {
                // Get overviews for the previous, current, and next month
                var currentMonthOverview = await GetADisbursementOverviewInMonth(project.Id, DateTime.Now);
                var previousMonthOverview = await GetADisbursementOverviewInMonth(project.Id, DateTime.Now.AddMonths(-1));
                var nextMonthOverview = await GetADisbursementOverviewInMonth(project.Id, DateTime.Now.AddMonths(1));

                // Create overview for each project
                var overview = new DisbursementOverviewOfProjectForInvestor
                {
                    Id = project.Id,
                    LogoUrl = project.LogoUrl,
                    ProjectName = project.ProjectName,
                    DisbursementInfo = new List<DisbursementOverviewOfProject>
                    {
                        previousMonthOverview,
                        currentMonthOverview,
                        nextMonthOverview
                    },
                };

                result.Add(overview);
            }

            // Return as paginated result
            return new PaginationDTO<DisbursementOverviewOfProjectForInvestor>
            {
                Data = result,
                Total = totalRecords,
                Page = page,
                Size = size
            };
        }

    }
}
