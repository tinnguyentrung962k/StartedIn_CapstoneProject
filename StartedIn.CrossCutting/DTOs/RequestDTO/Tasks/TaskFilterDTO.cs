using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Tasks;

public class TaskFilterDTO
{
    public string? Title { get; set; }
    public TaskEntityStatus? Status = null;
    public bool? IsLate = false;
    public string? AssigneeId { get; set; }
    public string? MilestoneId { get; set; }
}