using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone
{
    public class MilestoneResponseDTO : IdentityResponseDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateOnly DueDate { get; set; }
        public DateOnly? ExtendedDate { get; set; }
        public int? ExtendedCount { get; set; }
        public PhaseEnum PhaseName { get; set; }
    }
}
