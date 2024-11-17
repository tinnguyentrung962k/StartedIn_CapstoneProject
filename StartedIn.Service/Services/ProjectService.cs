using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Project;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Project;
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
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepository;
    private readonly IAzureBlobService _azureBlobService;
    private readonly IUserService _userService;
    private readonly IFinanceRepository _financeRepository;

    public ProjectService(
        IProjectRepository projectRepository, 
        IUnitOfWork unitOfWork, 
        UserManager<User> userManager, 
        ILogger<ProjectService> logger, 
        IEmailService emailService, 
        IUserRepository userRepository, 
        IAzureBlobService azureBlobService, 
        IUserService userService,
        IFinanceRepository financeRepository)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _emailService = emailService;
        _userRepository = userRepository;
        _azureBlobService = azureBlobService;
        _userService = userService;
        _financeRepository = financeRepository;
    }
    public async Task<Project> CreateNewProject(string userId, ProjectCreateDTO projectCreateDTO)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var createdProject = await _projectRepository
             .QueryHelper()
             .Include(x => x.UserProjects)
             .Filter(p => p.UserProjects.Any(up => up.UserId == userId && up.RoleInTeam == RoleInTeam.Leader))
             .GetOneAsync();
        if (createdProject is not null)
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
        if (projectCreateDTO.TotalShares < 0)
        {
            throw new InvalidDataException(MessageConstant.NegativeNumberError);
        }
        try {
            _unitOfWork.BeginTransaction();
            var imgUrl = await _azureBlobService.UploadAvatarOrCover(projectCreateDTO.LogoFile);
            var newProject = new Project
            {
                ProjectName = projectCreateDTO.ProjectName,
                Description = projectCreateDTO.Description,
                LogoUrl = imgUrl,
                ProjectStatus = ProjectStatusEnum.CONSTRUCTING,
                TotalShares = projectCreateDTO.TotalShares,
                EndDate = projectCreateDTO.EndDate,
                StartDate = projectCreateDTO.StartDate,
                CompanyIdNumber = projectCreateDTO.CompanyIdNumer
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

    public async Task<Project> GetProjectById(string id)
    {
        var project = await _projectRepository.GetProjectById(id);
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }

        return project;
    }

    public async Task<List<Project>> GetAllProjectsForAdmin(int page, int size)
    {
        var projects = await _projectRepository.QueryHelper().GetPagingAsync(page, size);
        return projects.ToList();
    }

    public async Task SendJoinProjectInvitation(string userId, List<ProjectInviteEmailAndRoleDTO> inviteUsers, string projectId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundUserError);
        }
        var project = await _projectRepository.GetProjectAndMemberByProjectId(projectId);
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }
        var userProject = project.UserProjects.FirstOrDefault(up => up.UserId.Equals(userId));
        if (userProject == null)
        {
            throw new NotFoundException(MessageConstant.UserNotInProjectError);
        }
        if (userProject.RoleInTeam != CrossCutting.Enum.RoleInTeam.Leader)
        {
            throw new InviteException(MessageConstant.RolePermissionError);
        }
        foreach (var inviteUser in inviteUsers)
        {
            await _emailService.SendInvitationToProjectAsync(inviteUser.Email, projectId, user.FullName, project.ProjectName,inviteUser.RoleInTeam);
        }
    }
    public async Task AddUserToProject(string projectId, string userId, RoleInTeam roleInTeam)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundUserError);
        }
        var project = await _projectRepository.QueryHelper().Include(x=>x.UserProjects).Filter(x=>x.Id.Equals(projectId)).GetOneAsync();
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }
        var existingLeader = project.UserProjects.FirstOrDefault(x => x.RoleInTeam == RoleInTeam.Leader);
        if (roleInTeam == RoleInTeam.Leader && existingLeader != null)
        {
            throw new InviteException(MessageConstant.JoinGroupWithLeaderRoleError);
        }
        var userInTeam = await _userRepository.GetAUserInProject(projectId, user.Id);
        if (userInTeam != null)
        {
            return;
        }
        await _userRepository.AddUserToProject(userId, projectId, roleInTeam);
        await _unitOfWork.SaveChangesAsync();
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

    public async Task<List<ProjectResponseDTO>> GetListOwnProjects(string userId)
    {
        var projects = _projectRepository.QueryHelper()
            .Include(x => x.UserProjects)
            .Filter(p => p.UserProjects.Any(up => up.UserId == userId && up.RoleInTeam == RoleInTeam.Leader));
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
                TotalShares = project.TotalShares,
                RemainingPercentOfShares = project.RemainingPercentOfShares,
                RemainingShares = project.RemainingShares,
                StartDate = project.StartDate,
                EndDate = project.EndDate
            };
            listProjects.Add(responseDto);
        }
        return listProjects;
    }

    public async Task<List<ProjectResponseDTO>> GetListParticipatedProjects(string userId)
    {
        var projects = _projectRepository.QueryHelper()
            .Include(x => x.UserProjects)
            .Filter(p => p.UserProjects.Any(up => up.UserId == userId && up.RoleInTeam != RoleInTeam.Leader));
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
                TotalShares = project.TotalShares,
                RemainingPercentOfShares = project.RemainingPercentOfShares,
                RemainingShares = project.RemainingShares,
                StartDate = project.StartDate,
                EndDate = project.EndDate
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

    public async Task<PaginationDTO<ExploreProjectDTO>> GetProjectsForInvestor(string userId, int size, int page)
    {
        var projects = _projectRepository.QueryHelper().Include(p => p.UserProjects)
            .Filter(p => !p.UserProjects.Any(up => up.UserId.Contains(userId))).OrderBy(x=>x.OrderByDescending(x=>x.StartDate));
        var result = await projects.GetPagingAsync(page, size);
        List<ExploreProjectDTO> exploreProjects = new List<ExploreProjectDTO>();
        foreach (var project in result)
        {
            foreach (var userProject in project.UserProjects)
            {
                var user = await _userManager.FindByIdAsync(userProject.UserId);
                userProject.User = user;
            }
            ExploreProjectDTO exploreProjectDTO = new ExploreProjectDTO
            {
                Description = project.Description,
                Id = project.Id,
                LeaderFullName = project.UserProjects.FirstOrDefault(x => x.RoleInTeam == RoleInTeam.Leader).User.FullName,
                LeaderId = project.UserProjects.FirstOrDefault(x => x.RoleInTeam == RoleInTeam.Leader).User.Id,
                LogoUrl = project.LogoUrl,
                ProjectName = project.ProjectName,
            };
            exploreProjects.Add(exploreProjectDTO);
        }
        var response = new PaginationDTO<ExploreProjectDTO>
        {
            Data = exploreProjects,
            Page = page,
            Size = size,
            Total = await projects.GetTotal()
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
            project.HarshClientIdPayOsKey = EncryptString(payOsPaymentGatewayRegisterDTO.ClientIdKey);
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

}