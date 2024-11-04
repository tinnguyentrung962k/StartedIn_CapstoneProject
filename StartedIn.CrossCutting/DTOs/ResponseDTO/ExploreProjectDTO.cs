namespace StartedIn.CrossCutting.DTOs.ResponseDTO;

public class ExploreProjectDTO : IdentityResponseDTO
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public string LogoUrl { get; set; }
    public string LeaderId { get; set; }
    public string LeaderFullName { get; set; }
}