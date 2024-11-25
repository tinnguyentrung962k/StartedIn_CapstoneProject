namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone;

public class MilestoneInPhaseResponseDTO
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int? Progress { get; set; }
}