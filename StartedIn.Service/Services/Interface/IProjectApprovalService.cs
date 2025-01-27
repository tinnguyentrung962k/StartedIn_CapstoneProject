using StartedIn.CrossCutting.DTOs.RequestDTO.ProjectApproval;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectApproval;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IProjectApprovalService
{
    Task<ProjectApprovalResponseDTO> CreateProjectApprovalRequest(string userId, string projectId, CreateProjectApprovalDTO createProjectApprovalDto);
    Task<List<ProjectApprovalResponseDTO>> GetProjectApprovalRequestByProjectId(string userId, string projectId);
    Task ApproveProjectRequest(string approvalId);
    Task RejectProjectRequest(string approvalId, CancelReasonApprovalDTO cancelReasonDTO);
    Task<PaginationDTO<ProjectApprovalResponseDTO>> GetAllProjectApprovals(ProjectApprovalFilterDTO filter, int page, int size);
    Task<ProjectApprovalResponseDTO> GetProjectApprovalRequestByApprovalId(string userId, string projectId, string approvalId);
    Task<ProjectApprovalResponseDTO> GetProjectApprovalRequestByApprovalIdForAdmin(string approvalId);
}