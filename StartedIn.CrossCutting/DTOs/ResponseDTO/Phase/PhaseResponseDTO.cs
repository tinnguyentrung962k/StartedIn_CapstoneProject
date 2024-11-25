using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Phase;

public class PhaseResponseDTO : IdentityResponseDTO
{
    public string PhaseName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string ProjectCharterId { get; set; }
    public List<MilestoneInPhaseResponseDTO> Milestones { get; set; }
}