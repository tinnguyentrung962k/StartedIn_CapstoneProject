using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;

public class CreateApplicationDTO
{
    public ApplicationStatus Status { get; set; }

    public ApplicationTypeEnum Type { get; set; }
    public RoleInTeam Role { get; set; }
    public IFormFile CV { get; set; }
}