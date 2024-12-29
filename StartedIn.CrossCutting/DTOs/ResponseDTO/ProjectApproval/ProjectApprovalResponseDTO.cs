using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Document;
using StartedIn.CrossCutting.DTOs.ResponseDTO.InvestmentCall;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectApproval;

public class ProjectApprovalResponseDTO : IdentityResponseDTO
{
    public string LeaderName { get; set; }
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string Reason { get; set; }
    public string? RejectReason { get; set; }
    public ProjectApprovalStatus Status { get; set; }
    public DateTimeOffset SentDate { get; set; }
    public DateTimeOffset? ApprovalDate { get; set; }
    public string TargetCall { get; set; }
    public string EquityShareCall { get; set; }
    public DateOnly? EndDate { get; set; }
    public List<DocumentResponseDTO> Documents { get; set; }
}