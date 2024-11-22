using StartedIn.CrossCutting.DTOs.BaseDTO;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Phase;

public class PhaseResponseDTO : IdentityResponseDTO
{
    public string PhaseName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string ProjectCharterId { get; set; }
}