namespace StartedIn.CrossCutting.DTOs.ResponseDTO;

public class ProjectListDTO
{
    public List<ProjectResponseDTO> listOwnProject { get; set; }
    public List<ProjectResponseDTO> listParticipatedProject { get; set; }
}