using StartedIn.CrossCutting.DTOs.BaseDTO;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Project;

public class ProjectInformationWithMembersResponseDTO : IdentityResponseDTO
{
    public string ProjectName { get; set; }
    public DateOnly StartDate { get; set; }
    public string Description { get; set; }
    public string? LogoUrl { get; set; }
    public List<MemberWithRoleInProjectResponseDTO> Members { get; set; }
}