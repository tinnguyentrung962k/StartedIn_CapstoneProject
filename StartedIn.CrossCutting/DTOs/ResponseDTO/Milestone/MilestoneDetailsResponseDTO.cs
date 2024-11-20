using StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone
{
    public class MilestoneDetailsResponseDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateOnly DueDate { get; set; }
        public DateOnly? ExtendedDate { get; set; }
        public int? ExtendedCount { get; set; }
        public PhaseEnum PhaseName { get; set; }
        public string CharterId { get; set; }
        public decimal Percentage { get; set; }
        public IEnumerable<TaskResponseDTO> AssignedTasks { get; set; }
    }
}
