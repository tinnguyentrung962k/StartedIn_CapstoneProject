using StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectCharter;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Project;

public class ProjectDetailForAdminDTO : ProjectResponseDTO
{
    public ProjectCharterResponseDTO ProjectCharterResponseDto { get; set; }
    public bool IsSignedInternalContract { get; set; }
}