using StartedIn.CrossCutting.DTOs.RequestDTO.ProjectApproval;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectApproval;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IProjectApprovalService
{
    Task<ProjectApproval> CreateProjectApprovalRequest(string userId, string projectId, CreateProjectApprovalDTO createProjectApprovalDto);
    Task<IEnumerable<ProjectApproval>> GetProjectApprovalRequestByProjectId(string userId, string projectId);
    Task ApproveProjectRequest(string projectId, string projectApprovalId);
    Task RejectProjectRequest(string projectId, string projectApprovalId, string rejectReason);
    Task<PaginationDTO<ProjectApprovalResponseDTO>> GetAllProjectApprovals(ProjectApprovalFilterDTO filter, int page, int size);
    Task<ProjectApproval> GetProjectApprovalRequestByApprovalId(string userId, string projectId, string approvalId);
}