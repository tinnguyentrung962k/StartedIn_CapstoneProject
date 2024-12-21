using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectApproval;

public class ProjectApprovalResponseDTO : IdentityResponseDTO
{
    public string LeaderName { get; set; }
    public string ProjectName { get; set; }
    public string Reason { get; set; }
    public ProjectApprovalStatus Status { get; set; }
    public DateTimeOffset SentDate { get; set; }
    public DateTimeOffset? ApprovalDate { get; set; }
}