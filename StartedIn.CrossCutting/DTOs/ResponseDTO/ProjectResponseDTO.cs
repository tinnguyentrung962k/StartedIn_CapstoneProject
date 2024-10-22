using StartedIn.CrossCutting.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO;

public class ProjectResponseDTO : IdentityResponseDTO
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public string ProjectStatus { get; set; }
    public string? LogoUrl { get; set; }
    public int? TotalShares { get; set; }
    public decimal RemainingPercentOfShares { get; set; }
    public int? RemainingShares { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}