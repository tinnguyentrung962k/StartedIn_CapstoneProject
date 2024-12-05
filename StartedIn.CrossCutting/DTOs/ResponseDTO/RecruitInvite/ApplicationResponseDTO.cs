using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.RecruitInvite;

public class ApplicationResponseDTO : IdentityResponseDTO
{
    public string CandidateId { get; set; }
    public string ProjectId { get; set; }
    public ApplicationStatus Status { get; set; }
    public ApplicationTypeEnum Type { get; set; }
    public RoleInTeam Role { get; set; }
    public string? CVUrl { get; set; }
}