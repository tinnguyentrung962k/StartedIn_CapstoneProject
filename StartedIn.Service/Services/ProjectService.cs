using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.DTOs.RequestDTO;
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

    public ProjectService(IProjectRepository projectRepository, IUnitOfWork unitOfWork, 
        UserManager<User> userManager, ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }
    public async Task<Project> CreateNewProject(string userId, ProjectCreateDTO projectCreateDto)
    {
        try
        {
            _unitOfWork.BeginTransaction();
            Project project = new Project
            {
                ProjectName = projectCreateDto.ProjectName,
                Description = projectCreateDto.Description,
                CreatedBy = userId
            };
            _projectRepository.Add(project);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return project;
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Error while creating project");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Project> GetProjectById(string id)
    {
        var project = await _projectRepository.GetProjectById(id);
        if (project == null)
        {
            throw new NotFoundException("No projects found");
        }

        return project;
    }

    public async Task SendJoinProjectInvitation(string userId, List<string> userIds, string projectId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID: {userId} not found.");
        }
        var project = await _projectRepository.QueryHelper()
            .Filter(project => project.Id.Equals(projectId))
            .Include(project => project.UserProjects)
            .Include()
            .GetOneAsync();
        if (project == null)
        {
            throw new NotFoundException("Không tìm thấy team");
        }
        if (project.UserProjects..Equals(user.Id))
        {
            throw new InviteException("Bạn không có quyền mời thành viên vào nhóm");
        }
        if (team.TeamUsers.Count() >= 5)
        {
            throw new TeamLimitException("Đội đã có đủ 5 thành viên. Vui lòng nâng cấp gói Premium");
        }
        if (team.TeamUsers.Count() + inviteEmails.Count > 5)
        {
            throw new TeamLimitException("Thêm người dùng này sẽ làm số thành viên của đội vượt quá 5 người. Vui lòng nâng cấp lên gói Premium");
        }
        foreach (var inviteEmail in inviteEmails)
        {
            _emailService.SendInvitationToTeam(inviteEmail, teamId);
        }
    }
}