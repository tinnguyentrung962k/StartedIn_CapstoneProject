namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Project;

public class ProjectListDTO
{
    public List<ProjectResponseDTO> listOwnProject { get; set; }
    public List<ProjectResponseDTO> listParticipatedProject { get; set; }
}