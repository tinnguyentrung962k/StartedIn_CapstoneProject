using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone
{
    public class MilestoneResponseDTO : IdentityResponseDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int? Progress { get; set; }
        public PhaseEnum PhaseName { get; set; }
    }
}
