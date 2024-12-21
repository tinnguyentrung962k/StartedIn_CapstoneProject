using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.ProjectApproval;

public class ProjectApprovalFilterDTO
{
    public ProjectApprovalStatus? Status { get; set; }
    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo { get; set; }
}