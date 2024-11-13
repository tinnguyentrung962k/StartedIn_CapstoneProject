using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.RequestDTO.Tasks;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;
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
        public DisbursementService(IDisbursementRepository disbursementRepository, IUnitOfWork unitOfWork, ILogger<Disbursement> logger, IProjectRepository projectRepository, IEmailService emailService, UserManager<User> userManager, IUserService userService, IMapper mapper)
        {
            _disbursementRepository = disbursementRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _projectRepository = projectRepository;
            _emailService = emailService;
            _userManager = userManager;
            _userService = userService;
            _mapper = mapper;
        }
        public async Task FinishedTheTransaction(string disbursementId, string projectId, string apiKey)
        {
           var project = await _projectRepository.GetProjectById(projectId);
           if (project == null)
           {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
           }
           var disbursement = await _disbursementRepository.GetDisbursementById(disbursementId);
           if (disbursement is null)
           {
                throw new NotFoundException(MessageConstant.DisbursementNotFound);
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
                _unitOfWork.BeginTransaction();
                disbursement.DisbursementStatus = CrossCutting.Enum.DisbursementStatusEnum.FINISHED;
                _disbursementRepository.Update(disbursement);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
           }
           catch (Exception ex) 
           {
                _logger.LogError($"An error occurred while updating a disbursement: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
           }

        }
        public async Task CancelPayment(string disbursementId, string projectId, string apiKey)
        {
            var project = await _projectRepository.GetProjectById(projectId);
            if (project == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }
            var disbursement = await _disbursementRepository.GetDisbursementById(disbursementId);
            if (disbursement is null)
            {
                throw new NotFoundException(MessageConstant.DisbursementNotFound);
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
                _unitOfWork.BeginTransaction();
                _logger.LogInformation($"Payment for Disbursement {disbursementId} under Project {projectId} has been requested to be canceled.");
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
        public async Task<PaginationDTO<DisbursementForLeaderInProjectResponseDTO>> GetDisbursementListForLeaderInAProject(string userId, string projectId, DisbursementFilterInProjectDTO disbursementFilterDTO, int size, int page)
        {
            var loginUser = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != CrossCutting.Enum.RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var filterDisbursements = _disbursementRepository.QueryHelper()
                .Include(x => x.Contract)
                .Include(x => x.Investor)
                .Filter(x => x.Contract.ProjectId.Equals(projectId) && x.IsValidWithContract == true).OrderBy(x=>x.OrderBy(x=>x.StartDate));
            if (!string.IsNullOrWhiteSpace(disbursementFilterDTO.Title))
            {
                filterDisbursements = filterDisbursements.Filter(d => d.Title != null && d.Title.ToLower().Contains(disbursementFilterDTO.Title.ToLower()));
            }
            if (disbursementFilterDTO.PeriodFrom.HasValue && disbursementFilterDTO.PeriodTo.HasValue)
            {
                filterDisbursements = filterDisbursements.Filter(d => d.StartDate >= disbursementFilterDTO.PeriodFrom.Value && d.EndDate <= disbursementFilterDTO.PeriodTo.Value);
            }
            else if (disbursementFilterDTO.PeriodFrom.HasValue)
            {
                filterDisbursements = filterDisbursements.Filter(d => d.StartDate >= disbursementFilterDTO.PeriodFrom.Value);
            }
            else if (disbursementFilterDTO.PeriodTo.HasValue)
            {
                filterDisbursements = filterDisbursements.Filter(d => d.EndDate <= disbursementFilterDTO.PeriodTo.Value);
            }

            // Amount range filter
            if (disbursementFilterDTO.AmountFrom.HasValue)
            {
                filterDisbursements = filterDisbursements.Filter(d => d.Amount >= disbursementFilterDTO.AmountFrom.Value);
            }
            if (disbursementFilterDTO.AmountTo.HasValue)
            {
                filterDisbursements = filterDisbursements.Filter(d => d.Amount <= disbursementFilterDTO.AmountTo.Value);
            }

            // Status filter
            if (disbursementFilterDTO.DisbursementStatus.HasValue)
            {
                filterDisbursements = filterDisbursements.Filter(d => d.DisbursementStatus == disbursementFilterDTO.DisbursementStatus.Value);
            }

            // Investor filter
            if (!string.IsNullOrEmpty(disbursementFilterDTO.InvestorId))
            {
                filterDisbursements = filterDisbursements.Filter(d => d.InvestorId.Equals(disbursementFilterDTO.InvestorId));
            }

            // Contract filter
            if (!string.IsNullOrWhiteSpace(disbursementFilterDTO.ContractId))
            {
                filterDisbursements = filterDisbursements.Filter(d => d.ContractId.Equals(disbursementFilterDTO.ContractId));
            }

            var pagination = new PaginationDTO<DisbursementForLeaderInProjectResponseDTO>()
            {
                Data = _mapper.Map<IEnumerable<DisbursementForLeaderInProjectResponseDTO>>(await filterDisbursements.GetPagingAsync(page, size)),
                Total = await filterDisbursements.GetTotal(),
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
            var filterDisbursements = _disbursementRepository.QueryHelper()
                .Include(x => x.Contract)
                .Include(x => x.Investor)
                .Filter(x => x.InvestorId.Equals(investor.Id) && x.IsValidWithContract == true)
                .OrderBy(x => x.OrderBy(x => x.StartDate));
            if (!string.IsNullOrWhiteSpace(disbursementFilterDTO.Title))
            {
                filterDisbursements = filterDisbursements.Filter(d => d.Title != null && d.Title.ToLower().Contains(disbursementFilterDTO.Title.ToLower()));
            }
            if (disbursementFilterDTO.PeriodFrom.HasValue && disbursementFilterDTO.PeriodTo.HasValue)
            {
                filterDisbursements = filterDisbursements.Filter(d => d.StartDate >= disbursementFilterDTO.PeriodFrom.Value && d.EndDate <= disbursementFilterDTO.PeriodTo.Value);
            }
            else if (disbursementFilterDTO.PeriodFrom.HasValue)
            {
                filterDisbursements = filterDisbursements.Filter(d => d.StartDate >= disbursementFilterDTO.PeriodFrom.Value);
            }
            else if (disbursementFilterDTO.PeriodTo.HasValue)
            {
                filterDisbursements = filterDisbursements.Filter(d => d.EndDate <= disbursementFilterDTO.PeriodTo.Value);
            }

            // Amount range filter
            if (disbursementFilterDTO.AmountFrom.HasValue)
            {
                filterDisbursements = filterDisbursements.Filter(d => d.Amount >= disbursementFilterDTO.AmountFrom.Value);
            }
            if (disbursementFilterDTO.AmountTo.HasValue)
            {
                filterDisbursements = filterDisbursements.Filter(d => d.Amount <= disbursementFilterDTO.AmountTo.Value);
            }

            // Status filter
            if (disbursementFilterDTO.DisbursementStatus.HasValue)
            {
                filterDisbursements = filterDisbursements.Filter(d => d.DisbursementStatus == disbursementFilterDTO.DisbursementStatus.Value);
            }

            // Project filter
            if (!string.IsNullOrEmpty(disbursementFilterDTO.ProjectId))
            {
                filterDisbursements = filterDisbursements.Filter(d => d.Contract.ProjectId.Equals(disbursementFilterDTO.ProjectId));
            }

            // Contract filter
            if (!string.IsNullOrWhiteSpace(disbursementFilterDTO.ContractId))
            {
                filterDisbursements = filterDisbursements.Filter(d => d.ContractId.Equals(disbursementFilterDTO.ContractId));
            }
            var recordInPage = await filterDisbursements.GetPagingAsync(page, size);
            foreach (var filterDisbursement in recordInPage)
            {
                var project = await _projectRepository.GetProjectById(filterDisbursement.Contract.ProjectId);
                filterDisbursement.Contract.Project = project;
            }    
            var pagination = new PaginationDTO<DisbursementForInvestorInInvestorMenuResponseDTO>()
            {
                Data = _mapper.Map<IEnumerable<DisbursementForInvestorInInvestorMenuResponseDTO>>(recordInPage),
                Total = await filterDisbursements.GetTotal(),
                Page = page,
                Size = size
            };

            return pagination;


        }


    }
}
