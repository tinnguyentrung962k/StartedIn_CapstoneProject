using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IProjectService 
{
    Task<Project> CreateNewProject(string userId, ProjectCreateDTO projectCreateDto);

    Task<Project> GetProjectById(string id);
    Task SendJoinProjectInvitation(string userId, List<string> userIds, string teamId);
}