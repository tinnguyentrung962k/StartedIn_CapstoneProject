using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class TaskResponseDTO : IdentityResponseDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public bool IsLate { get; set; }
        public TaskEntityStatus Status { get; set; }
        public DateTimeOffset? Deadline { get; set; }
    }
}
