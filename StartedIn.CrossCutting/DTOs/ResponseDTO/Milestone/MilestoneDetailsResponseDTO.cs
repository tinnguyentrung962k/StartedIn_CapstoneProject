using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Phase;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone
{
    public class MilestoneDetailsResponseDTO : IdentityResponseDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int? Progress { get; set; }
        public IEnumerable<TaskResponseDTO> AssignedTasks { get; set; }
        public PhaseResponseDTO Phase { get; set; }
    }
}
