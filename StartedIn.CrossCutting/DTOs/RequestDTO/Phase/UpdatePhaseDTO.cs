namespace StartedIn.CrossCutting.DTOs.RequestDTO.Phase;

public class UpdatePhaseDTO
{
    public string PhaseName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? ProjectCharterId { get; set; }
}