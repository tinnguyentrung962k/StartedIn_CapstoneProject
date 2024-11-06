using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;

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

    public ProjectService(IProjectRepository projectRepository, IUnitOfWork unitOfWork, 
        UserManager<User> userManager, ILogger<ProjectService> logger, IEmailService emailService, IUserRepository userRepository, IAzureBlobService azureBlobService)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _emailService = emailService;
        _userRepository = userRepository;
        _azureBlobService = azureBlobService;
    }
    public async Task CreateNewProject(string userId, Project project, IFormFile avatar)
    {

        try {
            _unitOfWork.BeginTransaction();
            var user = await _userManager.FindByIdAsync(userId);
            project.ProjectStatus = ProjectStatusEnum.CONSTRUCTING;
            var imgUrl = await _azureBlobService.UploadAvatarOrCover(avatar);
            project.LogoUrl = imgUrl;
            project.CreatedBy = user.FullName;
            var projectEntity = _projectRepository.Add(project);
            await _userRepository.AddUserToProject(userId, project.Id, RoleInTeam.Leader);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
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
            throw new NotFoundException("Không có dự án được tìm thấy");
        }

        return project;
    }

    public async Task SendJoinProjectInvitation(string userId, List<ProjectInviteEmailAndRoleDTO> inviteUsers, string projectId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException($"Người dùng ID: {userId} không tìm thấy.");
        }
        var project = await _projectRepository.GetProjectAndMemberByProjectId(projectId);
        if (project == null)
        {
            throw new NotFoundException("Không tìm thấy team");
        }
        var userProject = project.UserProjects.FirstOrDefault(up => up.UserId.Equals(userId));
        if (userProject == null)
        {
            throw new NotFoundException("Người dùng không thuộc dự án này");
        }
        if (userProject.RoleInTeam != CrossCutting.Enum.RoleInTeam.Leader)
        {
            throw new InviteException("Bạn không có quyền mời thành viên");
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
        if (existingLeader != null)
        {
            throw new InviteException(MessageConstant.JoinGroupWithLeaderRoleError);
        }
        var userInTeam = await _userRepository.GetAUserInProject(projectId, user.Id);
        if (userInTeam != null)
        {
            throw new InviteException("Người dùng đã có trong nhóm");
        }
        await _userRepository.AddUserToProject(userId, projectId, roleInTeam);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Project> GetProjectAndMemberById(string id)
    {
        var project = await _projectRepository.GetProjectAndMemberByProjectId(id);
        if (project == null) 
        {
            throw new NotFoundException("Không tìm thấy dự án");
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
            throw new NotFoundException("Không tìm thấy dự án");
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

    public async Task<SearchResponseDTO<ExploreProjectDTO>> GetProjectsForInvestor(string userId, int pageIndex, int pageSize)
    {
        var projects = _projectRepository.QueryHelper().Include(p => p.UserProjects)
            .Filter(p => !p.UserProjects.Any(up => up.UserId.Contains(userId))).OrderBy(x=>x.OrderByDescending(x=>x.StartDate));
        var records = await projects.GetAllAsync();
        var result = await projects.GetPagingAsync(pageIndex, pageSize);
        var totalRecord = records.Count();
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
        var response = new SearchResponseDTO<ExploreProjectDTO>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            ResponseList = exploreProjects,
            TotalPage = (int)Math.Ceiling((double)totalRecord / pageSize),
            TotalRecord = totalRecord
        };
        return response;
    }
}