using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IProjectService 
{
    Task CreateNewProject(string userId, Project project, IFormFile avatar);
    Task<Project> GetProjectById(string id);
    Task SendJoinProjectInvitation(string userId, List<string> inviteEmails, string projectId);
    Task AddUserToProject(string projectId, string userId);
    Task<Project> GetProjectAndMemberById(string id);
    Task<List<Project>> GetListOwnProjects(string userId);
    Task<List<Project>> GetListParticipatedProjects(string userId);
    Task<List<User>> GetListUserRelevantToContractsInAProject(string projectId);
    Task<IEnumerable<Project>> GetProjectsForInvestor(string userId, int pageIndex, int pageSize);
}