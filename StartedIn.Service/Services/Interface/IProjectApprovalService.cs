using StartedIn.CrossCutting.DTOs.RequestDTO.ProjectApproval;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IProjectApprovalService
{
    public Task<ProjectApproval> CreateProjectApprovalRequest(string userId, string projectId, CreateProjectApprovalDTO createProjectApprovalDto);
}