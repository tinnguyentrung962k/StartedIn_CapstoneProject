using StartedIn.CrossCutting.DTOs.RequestDTO.ProjectApproval;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectApproval;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IProjectApprovalService
{
    Task<ProjectApproval> CreateProjectApprovalRequest(string userId, string projectId, CreateProjectApprovalDTO createProjectApprovalDto);
    Task<ProjectApproval> GetProjectApprovalRequestByProjectId(string projectId);
    Task ApproveProjectRequest(string projectId);
    Task RejectProjectRequest(string projectId, string rejectReason);
    Task<PaginationDTO<ProjectApprovalResponseDTO>> GetAllProjectApprovals(ProjectApprovalFilterDTO filter, int page, int size);
}