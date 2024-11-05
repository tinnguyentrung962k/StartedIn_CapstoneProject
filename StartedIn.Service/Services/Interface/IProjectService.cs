using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IProjectService 
{
    Task CreateNewProject(string userId, Project project, IFormFile avatar);
    Task<Project> GetProjectById(string id);
    Task SendJoinProjectInvitation(string userId, List<ProjectInviteEmailAndRoleDTO> inviteUsers, string projectId);
    Task AddUserToProject(string projectId, string userId, RoleInTeam roleInTeam);
    Task<Project> GetProjectAndMemberById(string id);
    Task<List<Project>> GetListOwnProjects(string userId);
    Task<List<Project>> GetListParticipatedProjects(string userId);
    Task<List<User>> GetListUserRelevantToContractsInAProject(string projectId);
    Task<SearchResponseDTO<ExploreProjectDTO>> GetProjectsForInvestor(string userId, int pageIndex, int pageSize);
}