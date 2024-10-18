namespace StartedIn.CrossCutting.DTOs.ResponseDTO;

public class ProjectResponseDTO : IdentityResponseDTO
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public string ProjectStatus { get; set; }   
}