using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectApproval;

public class ApprovalRequestsResponseDTO : IdentityResponseDTO
{
    public string ProjectId { get; set; }
    public ProjectApprovalStatus Status { get; set; }
    public string? RejectReason { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset LastUpdatedTime { get; set; }
}