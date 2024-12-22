using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Project;

public class ProjectResponseDTO : IdentityResponseDTO
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public string LeaderId { get; set; }
    public string LeaderFullName { get; set; }
    public string? LeaderProfilePicture { get; set; }
    public ProjectStatusEnum ProjectStatus { get; set; }
    public string? LogoUrl { get; set; }
    public decimal RemainingPercentOfShares { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int MaxMember { get; set; }
    public int CurrentMember { get; set; }

}