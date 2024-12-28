using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Tasks;

public class TaskFilterDTO
{
    public string? Title { get; set; }
    public TaskEntityStatus? Status { get; set; }
    public bool? IsLate { get; set; }
    public string? AssigneeId { get; set; }
    public string? MilestoneId { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool? Priority { get; set; }
}