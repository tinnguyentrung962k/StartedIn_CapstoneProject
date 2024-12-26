using AutoMapper;
using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Project;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Asset;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Contract;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.ResponseDTO.InvestmentCall;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Project;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using System.Security.Cryptography;
using System.Text;

namespace StartedIn.Service.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ProjectService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IAzureBlobService _azureBlobService;
    private readonly IUserService _userService;
    private readonly IFinanceRepository _financeRepository;
    private readonly IMilestoneService _milestoneService;
    private readonly IShareEquityService _shareEquityService;
    private readonly ITransactionService _transactionService;
    private readonly IDisbursementService _disbursementService;
    private readonly IDisbursementRepository _disbursementRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IMapper _mapper;
    private readonly IInvestmentCallRepository _investmentCallRepository;
    private readonly IEmailService _emailService;
    private readonly IAssetRepository _assetRepository;
    private readonly ILeavingRequestRepository _leavingRequestRepository;
    private readonly ITaskRepository _taskRepository;

    public ProjectService(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        ILogger<ProjectService> logger,
        IUserRepository userRepository,
        IAzureBlobService azureBlobService,
        IUserService userService,
        IFinanceRepository financeRepository,
        IMilestoneService milestoneService,
        IShareEquityService shareEquityService,
        ITransactionService transactionService,
        IDisbursementService disbursementService,
        IContractRepository contractRepository,
        IInvestmentCallRepository investmentCallRepository,
        IEmailService emailService,
        IDisbursementRepository disbursementRepository,
        IAssetRepository assetRepository,
        ILeavingRequestRepository leavingRequestRepository,
        ITaskRepository taskRepository,
        IMapper mapper)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _userRepository = userRepository;
        _azureBlobService = azureBlobService;
        _userService = userService;
        _financeRepository = financeRepository;
        _milestoneService = milestoneService;
        _shareEquityService = shareEquityService;
        _transactionService = transactionService;
        _disbursementService = disbursementService;
        _contractRepository = contractRepository;
        _disbursementRepository = disbursementRepository;
        _mapper = mapper;
        _investmentCallRepository = investmentCallRepository;
        _emailService = emailService;
        _assetRepository = assetRepository;
        _leavingRequestRepository = leavingRequestRepository;
        _taskRepository = taskRepository;
    }
    public async Task<Project> CreateNewProject(string userId, ProjectCreateDTO projectCreateDTO)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var createdProject = await _projectRepository
             .QueryHelper()
             .Include(x => x.UserProjects)
             .Filter(p => p.UserProjects.Any(up => up.UserId == userId && up.RoleInTeam == RoleInTeam.Leader))
             .GetOneAsync();
        var existingProject = await _projectRepository.QueryHelper()
            .Filter(p => p.ProjectName.ToLower().Contains(projectCreateDTO.ProjectName.ToLower())).GetOneAsync();
        if (existingProject != null)
        {
            throw new InvalidDataException(MessageConstant.ExistProjectName);
        }
        var userInProject = await _projectRepository.GetAProjectByUserId(user.Id);
        if (createdProject != null || userInProject != null)
        {
            throw new ExistedRecordException(MessageConstant.CreateMoreProjectError);
        }
        if (string.IsNullOrWhiteSpace(projectCreateDTO.ProjectName))
        {
            throw new InvalidDataException(MessageConstant.NullOrWhiteSpaceProjectName);
        }
        if (string.IsNullOrWhiteSpace(projectCreateDTO.Description))
        {
            throw new InvalidDataException(MessageConstant.NullOrWhiteSpaceDescription);
        }
        if (projectCreateDTO.LogoFile == null)
        {
            throw new InvalidDataException(MessageConstant.NullOrEmptyLogoFile);
        }
        if (projectCreateDTO.StartDate == null)
        {
            throw new InvalidDataException(MessageConstant.NullOrEmptyStartDate);
        }
        
        try
        {
            _unitOfWork.BeginTransaction();
            var imgUrl = await _azureBlobService.UploadAvatarOrCover(projectCreateDTO.LogoFile);
            var newProject = new Project
            {
                ProjectName = projectCreateDTO.ProjectName,
                Description = projectCreateDTO.Description,
                LogoUrl = imgUrl,
                ProjectStatus = ProjectStatusEnum.CONSTRUCTING,
                EndDate = projectCreateDTO.EndDate,
                StartDate = projectCreateDTO.StartDate,
                CreatedBy = user.FullName
            };
            var projectEntity = _projectRepository.Add(newProject);
            await _userRepository.AddUserToProject(userId, projectEntity.Id, RoleInTeam.Leader);
            var projectFinance = new Finance
            {
                Project = projectEntity,
                ProjectId = projectEntity.Id
            };
            var projectFinanaceEntity = _financeRepository.Add(projectFinance);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return projectEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating project.");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateProjectDetail(string userId, string projectId, ProjectDetailPostDTO projectDetail) 
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        try
        {
            var project = await _projectRepository.GetProjectById(projectId);
            if (project == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }
            project.ProjectDetailPost = projectDetail.ProjectDetailPost;
            _projectRepository.Update(project);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MessageConstant.UpdateFailed);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task AddAppointmentUrl(string userId, string projectId, AppointmentUrlDTO appointmentUrlDto)
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
        if (projectRole != RoleInTeam.Leader)
        {
            throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
        }

        try
        {
            var project = await _projectRepository.QueryHelper().Filter(p => p.Id.Equals(projectId)).GetOneAsync();
            if (project == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }

            project.AppointmentUrl = appointmentUrlDto.AppointmentUrl;
            _projectRepository.Update(project);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while setting project appointment link.");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Project> GetProjectById(string id)
    {
        var project = await _projectRepository.GetProjectById(id);
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }

        return project;
    }

    public async Task<PaginationDTO<ProjectResponseDTO>> GetAllProjectsForAdmin(ProjectAdminFilterDTO projectAdminFilterDTO,int page, int size)
    {
        var projects = _projectRepository.GetProjectListQuery();
        if (!string.IsNullOrWhiteSpace(projectAdminFilterDTO.ProjectName))
        {
            projects = projects.Where(x => x.ProjectName.ToLower().Contains(projectAdminFilterDTO.ProjectName.ToLower()));
        }
        if (!string.IsNullOrWhiteSpace(projectAdminFilterDTO.Description))
        {
            projects = projects.Where(x => x.Description.ToLower().Contains(projectAdminFilterDTO.Description.ToLower()));
        }
        if (!string.IsNullOrWhiteSpace(projectAdminFilterDTO.LeaderFullName))
        {
            projects = projects.Where(x => x.UserProjects.FirstOrDefault(x => x.RoleInTeam.Equals(RoleInTeam.Leader)).User.FullName.ToLower().Contains(projectAdminFilterDTO.LeaderFullName.ToLower()));
        }
        if (projectAdminFilterDTO.Status != null)
        {
            projects = projects.Where(x => x.ProjectStatus == projectAdminFilterDTO.Status);
        }
        int totalCount = await projects.CountAsync();
        var pagedResult = await projects
            .Include(p => p.UserProjects)
            .ThenInclude(up => up.User)
            .Include(p => p.InvestmentCalls)
            .Include(p => p.Finance)
            .Include(p => p.ProjectCharter)
            .ThenInclude(pc => pc.Phases)
            .Include(p => p.Contracts)
            .ThenInclude(c => c.Disbursements)
            .Include(x => x.Milestones)
            .ThenInclude(x => x.Tasks)
            .Where(x => x.DeletedTime == null)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var projectResponseDTOs = pagedResult.Select(project => new ProjectResponseDTO
        {
            Description = project.Description,
            Id = project.Id,
            LeaderFullName = project.UserProjects.FirstOrDefault(x => x.RoleInTeam == RoleInTeam.Leader).User.FullName,
            LeaderId = project.UserProjects.FirstOrDefault(x => x.RoleInTeam == RoleInTeam.Leader).User.Id,
            LogoUrl = project.LogoUrl,
            ProjectName = project.ProjectName,
            ProjectStatus = project.ProjectStatus,
            RemainingPercentOfShares = project.RemainingPercentOfShares,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            ProjectDetailPost = project.ProjectDetailPost,
            CurrentMember = project.UserProjects.Where(x => (x.RoleInTeam == RoleInTeam.Leader || x.RoleInTeam == RoleInTeam.Member) && x.Status == UserStatusInProject.Active).Count()
        }).ToList();
        var pagination = new PaginationDTO<ProjectResponseDTO>()
        {
            Data = _mapper.Map<List<ProjectResponseDTO>>(pagedResult),
            Total = totalCount,
            Page = page,
            Size = size
        };

        return pagination;

    }

    public async Task<Project> GetProjectAndMemberById(string id)
    {
        var project = await _projectRepository.GetProjectAndMemberByProjectId(id);
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }
        return project;

    }

    public async Task<List<ProjectResponseDTO>> GetListParticipatedProjects(string userId)
    {
        var projects = _projectRepository.QueryHelper()
            .Include(x => x.UserProjects)
            .Filter(p => p.UserProjects.Any(up => up.UserId == userId));
        var records = await projects.GetAllAsync();
        var totalRecord = records.Count();
        List<ProjectResponseDTO> listProjects = new List<ProjectResponseDTO>();
        foreach (var project in records)
        {
            foreach (var userProject in project.UserProjects)
            {
                var user = await _userManager.FindByIdAsync(userProject.UserId);
                userProject.User = user;
            }
            ProjectResponseDTO responseDto = new ProjectResponseDTO
            {
                Description = project.Description,
                Id = project.Id,
                LeaderFullName = project.UserProjects.FirstOrDefault(x => x.RoleInTeam == RoleInTeam.Leader).User.FullName,
                LeaderId = project.UserProjects.FirstOrDefault(x => x.RoleInTeam == RoleInTeam.Leader).User.Id,
                LogoUrl = project.LogoUrl,
                ProjectName = project.ProjectName,
                ProjectStatus = project.ProjectStatus,
                RemainingPercentOfShares = project.RemainingPercentOfShares,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                CurrentMember = project.UserProjects.Count(),
                LeaderProfilePicture = project.UserProjects.FirstOrDefault(x => x.RoleInTeam == RoleInTeam.Leader).User.ProfilePicture,
                UserStatusInProject = project.UserProjects.FirstOrDefault(x => x.UserId.Equals(userId)).Status,
                ProjectDetailPost = project.ProjectDetailPost
            };
            listProjects.Add(responseDto);
        }
        return listProjects;
    }
    public async Task<List<User>> GetListUserRelevantToContractsInAProject(string projectId)
    {
        var project = await _projectRepository.GetOneAsync(projectId);
        if (project is null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }

        var userContractsList = await _userRepository.GetUsersListRelevantToContractsInAProject(projectId);
        var uniqueUsers = new HashSet<string>(); // To track unique user IDs
        var listUser = new List<User>();

        foreach (var userContract in userContractsList)
        {
            var user = userContract.User;
            if (user != null && uniqueUsers.Add(user.Id)) // Add only if ID is unique
            {
                listUser.Add(user);
            }
        }

        return listUser;
    }

    public async Task<PaginationDTO<ExploreProjectDTO>> GetProjectsForInvestor(string userId, ProjectFilterDTO projectFilterDTO,int size, int page)
    {
        var projects = _projectRepository.GetProjectListQueryForInvestor(userId).Where(x=>x.ActiveCallId != null);

        // Filter by project name
        if (!string.IsNullOrWhiteSpace(projectFilterDTO.ProjectName))
        {
            projects = projects.Where(x => x.ProjectName.ToLower().Contains(projectFilterDTO.ProjectName.ToLower()));
        }

        // Filter by active InvestmentCall status and apply additional filters if status is "open"
        if (projectFilterDTO.Status != null)
        {
            projects = projects.Where(x =>
                x.ActiveCallId != null &&
                x.InvestmentCalls.Any(ic => ic.Id == x.ActiveCallId && ic.Status == projectFilterDTO.Status));

            // Additional filters if the call status is "open"
            if (projectFilterDTO.Status == InvestmentCallStatus.Open)
            {
                if (projectFilterDTO.TargetFrom.HasValue)
                {
                    projects = projects.Where(x => x.InvestmentCalls.Any(ic => ic.Id == x.ActiveCallId && ic.TargetCall >= projectFilterDTO.TargetFrom.Value));
                }

                if (projectFilterDTO.TargetTo.HasValue)
                {
                    projects = projects.Where(x => x.InvestmentCalls.Any(ic => ic.Id == x.ActiveCallId && ic.TargetCall <= projectFilterDTO.TargetTo.Value));
                }

                if (projectFilterDTO.RaisedFrom.HasValue)
                {
                    projects = projects.Where(x => x.InvestmentCalls.Any(ic => ic.Id == x.ActiveCallId && ic.AmountRaised >= projectFilterDTO.RaisedFrom.Value));
                }

                if (projectFilterDTO.RaisedTo.HasValue)
                {
                    projects = projects.Where(x => x.InvestmentCalls.Any(ic => ic.Id == x.ActiveCallId && ic.AmountRaised <= projectFilterDTO.RaisedTo.Value));
                }

                if (projectFilterDTO.AvailableShareFrom.HasValue)
                {
                    projects = projects.Where(x => x.InvestmentCalls.Any(ic => ic.Id == x.ActiveCallId && ic.EquityShareCall >= projectFilterDTO.AvailableShareFrom.Value));
                }

                if (projectFilterDTO.AvailableShareTo.HasValue)
                {
                    projects = projects.Where(x => x.InvestmentCalls.Any(ic => ic.Id == x.ActiveCallId && ic.EquityShareCall <= projectFilterDTO.AvailableShareTo.Value));
                }
            }
        }
        int totalCount = await projects.CountAsync();

        var pagedResult = await projects
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
        List<ExploreProjectDTO> exploreProjects = new List<ExploreProjectDTO>();
        foreach (var project in pagedResult)
        {
            foreach (var userProject in project.UserProjects)
            {
                var user = await _userManager.FindByIdAsync(userProject.UserId);
                userProject.User = user;
            }
            var newestInvestmentCall = _mapper.Map<InvestmentCallResponseDTO>(await _investmentCallRepository.QueryHelper()
                .Filter(x => x.Id.Equals(project.ActiveCallId))
                .Include(x => x.DealOffers).GetOneAsync());
           
            ExploreProjectDTO exploreProjectDTO = new ExploreProjectDTO
            {
                Description = project.Description,
                Id = project.Id,
                LeaderFullName = project.UserProjects.FirstOrDefault(x => x.RoleInTeam == RoleInTeam.Leader).User.FullName,
                LeaderId = project.UserProjects.FirstOrDefault(x => x.RoleInTeam == RoleInTeam.Leader).User.Id,
                LogoUrl = project.LogoUrl,
                ProjectName = project.ProjectName,
                LeaderProfilePicture = project.UserProjects.FirstOrDefault(x => x.RoleInTeam == RoleInTeam.Leader).User.ProfilePicture,
                InvestmentCall = newestInvestmentCall,
                ProjectDetailPost = project.ProjectDetailPost
            };
            exploreProjects.Add(exploreProjectDTO);
        }
        var response = new PaginationDTO<ExploreProjectDTO>
        {
            Data = exploreProjects,
            Page = page,
            Size = size,
            Total = totalCount
        };
        return response;
    }

    public async Task AddPaymentGatewayInfo(string userId, string projectId, PayOsPaymentGatewayRegisterDTO payOsPaymentGatewayRegisterDTO)
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
        if (projectRole != RoleInTeam.Leader)
        {
            throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
        }
        try
        {
            var project = await _projectRepository.GetProjectById(projectId);
            project.HarshChecksumPayOsKey = EncryptString(payOsPaymentGatewayRegisterDTO.ChecksumKey);
            project.HarshClientIdPayOsKey = EncryptString(payOsPaymentGatewayRegisterDTO.ClientKey);
            project.HarshPayOsApiKey = EncryptString(payOsPaymentGatewayRegisterDTO.ApiKey);
            var projectEntity = _projectRepository.Update(project);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while register payment gateway.");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
    public async Task<PayOsPaymentGatewayResponseDTO> GetPaymentGatewayInfoByProjectId(string userId, string projectId)
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
        if (projectRole != RoleInTeam.Leader)
        {
            throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
        }
        var project = await _projectRepository.GetProjectById(projectId);
        PayOsPaymentGatewayResponseDTO payOsPaymentGatewayResponseDTO = new PayOsPaymentGatewayResponseDTO
        {
            ProjectId = project.Id,
            ApiKey = DecryptString(project.HarshPayOsApiKey),
            ChecksumKey = DecryptString(project.HarshChecksumPayOsKey),
            ClientKey = DecryptString(project.HarshClientIdPayOsKey)
        };
        return payOsPaymentGatewayResponseDTO;
    }

    public async Task<Project> ActivateProject(string projectId)
    {
        var project = await _projectRepository.QueryHelper().Filter(p => p.Id.Equals(projectId)).GetOneAsync();
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }

        try
        {
            _unitOfWork.BeginTransaction();
            project.ProjectStatus = ProjectStatusEnum.ACTIVE;
            _projectRepository.Update(project);
            await _unitOfWork.CommitAsync();
            await _unitOfWork.SaveChangesAsync();
            return project;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while activating project.");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
    private string EncryptString(string plainText)
    {
        // Use a key and IV (initialization vector) here - ideally these should be securely stored or generated
        var key = Encoding.UTF8.GetBytes("1234567890abcdef"); // Must be 16 bytes for AES-128
        var iv = Encoding.UTF8.GetBytes("abcdef0123456789"); // Must be 16 bytes for AES

        using (var aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cs))
                {
                    writer.Write(plainText);
                }

                // Ensure all data is written and flush the final encrypted result
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    // Helper method to decrypt a string
    private string DecryptString(string cipherText)
    {
        var key = Encoding.UTF8.GetBytes("1234567890abcdef"); // Same key as in EncryptString
        var iv = Encoding.UTF8.GetBytes("abcdef0123456789"); // Same IV as in EncryptString

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

    public async Task<ProjectDashboardDTO> GetProjectDashboard(string userId, string projectId)
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        var project = await _projectRepository.GetProjectById(projectId);
        var milestoneProgressList = new List<MilestoneProgressResponseDTO>();
        if (project.Milestones != null)
        {
            milestoneProgressList = project.Milestones.Where(m => m.DeletedTime == null)
                .Select(m => new MilestoneProgressResponseDTO
            {
                Id = m.Id,
                Title = m.Title,
                Progress = _milestoneService.CalculateProgress(m)
            }).ToList();
        }
        var transactionStatisticOfCurrentMonth = await _transactionService.GetInAndOutMoneyTransactionOfCurrentMonth(projectId);
        var userShareInProject = await _shareEquityService.GetShareEquityOfAUserInAProject(userId, projectId);

        var lateTask = await _taskRepository.GetTaskListInAProjectQuery(projectId).Where(x => x.IsLate == true).ToListAsync();
        var completedTask = await _taskRepository.GetTaskListInAProjectQuery(projectId).Where(x => x.Status == TaskEntityStatus.DONE).ToListAsync();
        var alltasks = await _taskRepository.GetTaskListInAProjectQuery(projectId).ToListAsync();
        int totalTask = alltasks.Count();
        ProjectDashboardDTO projectDashboardDTO = new ProjectDashboardDTO
        {
            CurrentBudget = project.Finance.CurrentBudget.ToString(),
            DisbursedAmount = project.Finance.DisbursedAmount.ToString(),
            RemainingDisbursement = project.Finance.RemainingDisbursement.ToString(),
            MilestoneProgress = milestoneProgressList,
            ShareEquityPercentage = userShareInProject.ToString(),
            InAmount = transactionStatisticOfCurrentMonth.InMoney.ToString(),
            OutAmount = transactionStatisticOfCurrentMonth.OutMoney.ToString(),
            CompletedTasks = _mapper.Map<List<TaskResponseDTO>>(completedTask),
            OverdueTasks = _mapper.Map<List<TaskResponseDTO>>(lateTask),
            TotalTask = totalTask
        };

        if (userInProject.RoleInTeam == RoleInTeam.Investor)
        {
            var selfDisbursementsStatistic = await _disbursementService.GetSelfDisbursementForInvestor(userId, projectId);
            projectDashboardDTO.SelfRemainingDisbursement = selfDisbursementsStatistic.SelfRemainingDisbursement;
            projectDashboardDTO.SelfDisbursedAmount = selfDisbursementsStatistic.SelfDisbursedAmount;
        }
        
        return projectDashboardDTO;
    }

    public async Task CloseAProject(string userId, string projectId)
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        if (userInProject.RoleInTeam != RoleInTeam.Leader)
        {
            throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
        }
        var contractList = await _contractRepository.QueryHelper()
            .Filter(x=>x.ProjectId == projectId)
            .Include(x=>x.Disbursements)
            .GetAllAsync();
        if (contractList.Any(x => (x.ContractStatus == ContractStatusEnum.SENT || x.ContractStatus == ContractStatusEnum.COMPLETED) 
        && x.ContractType != ContractTypeEnum.LIQUIDATIONNOTE))
        {
            throw new UpdateException(MessageConstant.ValidContractsStillExisted);
        }
        foreach (var contract in contractList)
        {
            if (contract.ContractType == ContractTypeEnum.INVESTMENT)
            {
                if (contract.Disbursements.Any(x => x.DisbursementStatus == DisbursementStatusEnum.OVERDUE 
                || x.DisbursementStatus == DisbursementStatusEnum.ERROR 
                || x.DisbursementStatus == DisbursementStatusEnum.ACCEPTED
                || x.DisbursementStatus == DisbursementStatusEnum.PENDING))
                {
                    throw new UpdateException(MessageConstant.DisbursementIssueExisted);
                    break;
                }
            }
        }
        var assetInProject = await _assetRepository.QueryHelper()
            .Filter(x => x.ProjectId.Equals(projectId)
            && x.DeletedTime == null
            && (x.Status != AssetStatus.Sold
            || (x.Status == AssetStatus.Unavailable && x.RemainQuantity > 0)))
            .GetAllAsync();
        if (assetInProject.Any())
        {
            throw new UpdateException(MessageConstant.UnsoldAssetsError);
        }
        try
        {
            _unitOfWork.BeginTransaction();
            var project = await _projectRepository.GetProjectById(projectId);
            foreach (var participant in project.UserProjects.Where(x => x.RoleInTeam != RoleInTeam.Leader))
            {
                participant.Status = UserStatusInProject.Left;
                await _userRepository.UpdateUserInProject(participant);
                var user = await _userManager.FindByIdAsync(participant.UserId);
                await _emailService.SendClosingProject(user.Email, userInProject.User.FullName, user.FullName, project.ProjectName);
            }
            project.ProjectStatus = ProjectStatusEnum.CLOSED;
            project.LastUpdatedTime = DateTime.UtcNow;
            project.LastUpdatedBy = userInProject.User.FullName;
            foreach (var userProject in project.UserProjects.Where(x=>x.Status != UserStatusInProject.Left))
            {
                userProject.Status = UserStatusInProject.Left;
                await _userRepository.UpdateUserInProject(userProject);
            }
            _projectRepository.Update(project);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while closing project.");
            await _unitOfWork.RollbackAsync();
            throw;
        }
        
    }

    public async Task<ClosingProjectInformationDTO> GetProjectClosingInformation(string userId, string projectId)
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        if (userInProject.RoleInTeam != RoleInTeam.Leader)
        {
            throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
        }
        var project = await _projectRepository.GetProjectById(projectId);
        var validContracts = await _contractRepository.QueryHelper()
            .Filter(x=>x.ProjectId.Equals(projectId) 
            && (x.ContractStatus == ContractStatusEnum.COMPLETED || x.ContractStatus == ContractStatusEnum.SENT || x.ContractStatus == ContractStatusEnum.WAITINGFORLIQUIDATION)
            && x.ContractType != ContractTypeEnum.LIQUIDATIONNOTE)
            .Include(x=>x.Disbursements)
            .GetAllAsync();

        var processingDisbursements = await _disbursementRepository.QueryHelper()
            .Include(x => x.Contract)
            .Filter(x => x.Contract.ProjectId.Equals(projectId) 
            && (x.DisbursementStatus == DisbursementStatusEnum.OVERDUE 
            || x.DisbursementStatus == DisbursementStatusEnum.ERROR 
            || x.DisbursementStatus == DisbursementStatusEnum.ACCEPTED 
            || x.DisbursementStatus == DisbursementStatusEnum.PENDING))
            .GetAllAsync();

        var assetInProject = await _assetRepository.QueryHelper()
            .Filter(x => x.ProjectId.Equals(projectId) && x.DeletedTime == null 
            && (x.Status != AssetStatus.Sold 
            || (x.Status == AssetStatus.Unavailable && x.RemainQuantity > 0)))
            .GetAllAsync();

        var closingProject = new ClosingProjectInformationDTO
        {
            CurrentBudget = project.Finance.CurrentBudget,
            Assets = _mapper.Map<List<AssetInClosingProjectDTO>>(assetInProject),
            Contracts = _mapper.Map<List<ContractInClosingProjectDTO>>(validContracts),
            Disbursements = _mapper.Map<List<DisbursementInClosingProjectDTO>>(processingDisbursements)
        };
        return closingProject;

    }

    public async Task<LeavingProjectInfomationDTO> GetProjectLeavingInformation(string userId, string projectId)
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        if (userInProject.RoleInTeam == RoleInTeam.Leader) 
        {
            throw new InvalidOperationException(MessageConstant.LeaderCannotLeaveGroup);
        }
        var project = await _projectRepository.GetProjectById(projectId);
        bool isExistingRequest = false;
        var existedLeavingRequest = await _leavingRequestRepository.QueryHelper()
                .Include(x=>x.Project)
                .Include(x=>x.User)
                .Filter(x => x.ProjectId.Equals(projectId)
                && x.UserId.Equals(userId)
                && x.Status == LeavingRequestStatus.PENDING)
                .GetOneAsync();
        if (existedLeavingRequest != null)
        {
            isExistingRequest = true;
        }
        var validContracts = await _contractRepository.QueryHelper()
            .Filter(x => x.ProjectId.Equals(projectId) 
            && (x.ContractStatus == ContractStatusEnum.COMPLETED || x.ContractStatus == ContractStatusEnum.SENT)
            && (x.UserContracts.Any(x => x.UserId.Equals(userId))) 
            && x.ContractType != ContractTypeEnum.LIQUIDATIONNOTE)
            .Include(x => x.Disbursements)
            .Include(x => x.UserContracts)
            .GetAllAsync();
        var disbursementList = validContracts.SelectMany(c => c.Disbursements)
            .Where(d => d.DisbursementStatus == DisbursementStatusEnum.OVERDUE
            || d.DisbursementStatus == DisbursementStatusEnum.ACCEPTED
            || d.DisbursementStatus == DisbursementStatusEnum.ERROR
            || d.DisbursementStatus == DisbursementStatusEnum.PENDING)
        .ToList();
        var leavingProject = new LeavingProjectInfomationDTO
        {
            Contracts = _mapper.Map<List<ContractInClosingProjectDTO>>(validContracts),
            Disbursements = _mapper.Map<List<DisbursementInClosingProjectDTO>>(disbursementList),
            RequestExisted = isExistingRequest  
        };
        return leavingProject;
    }

    public async Task<ProjectInformationWithMembersResponseDTO> GetProjectInformationWithMemberById(string projectId)
    {
        var project = await _projectRepository.GetProjectAndMemberByProjectId(projectId);
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }
        
        var listMember = new List<MemberWithRoleInProjectResponseDTO>();
        foreach (var member in project.UserProjects)
        {
            var memberResponse = new MemberWithRoleInProjectResponseDTO
            {
                FullName = member.User.FullName,
                RoleInTeam = member.RoleInTeam,
                Email = member.User.Email,
                Id = member.UserId
                
            };
            listMember.Add(memberResponse);
        }
        var response = new ProjectInformationWithMembersResponseDTO
        {
            ProjectName = project.ProjectName,
            StartDate = project.StartDate,
            Description = project.Description,
            LogoUrl = project.LogoUrl,
            Members = listMember,
            Id = project.Id
        };
        return response;
    }

    public async Task<List<ProjectResponseDTO>> GetProjectsThatUserLeft(string userId)
    {
        var leftProjects = _projectRepository.GetProjectsThatUserLeft(userId).ToList();
        var response = _mapper.Map<List<ProjectResponseDTO>>(leftProjects);
        return response;
    }

    public async Task<List<ProjectResponseDTO>> GetClosedProjectsForUser(string userId)
    {
        var leftProjects = _projectRepository.GetClosedProjectsForUser(userId).ToList();
        var response = _mapper.Map<List<ProjectResponseDTO>>(leftProjects);
        return response;
    }
    
}