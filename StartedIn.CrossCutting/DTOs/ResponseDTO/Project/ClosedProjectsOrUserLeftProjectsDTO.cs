namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Project;

public class ClosedProjectsOrUserLeftProjectsDTO
{
    public List<ProjectResponseDTO> ClosedProjects { get; set; }
    public List<ProjectResponseDTO> UserLeftProjects { get; set; }
}