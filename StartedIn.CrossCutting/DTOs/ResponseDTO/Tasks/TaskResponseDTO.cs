using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Project;
using StartedIn.CrossCutting.Enum;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks
{
    public class TaskResponseDTO : AuditResponseDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public bool IsLate { get; set; }
        public int ExpectedManHour { get; set; }
        public int? Priority { get; set; }
        public TaskEntityStatus Status { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public DateTimeOffset? ActualFinishAt { get; set; }
        public float? ActualManHour { get; set; }
        public ICollection<AssigneeInTaskDTO> Assignees { get; set; } = [];
    }
}
