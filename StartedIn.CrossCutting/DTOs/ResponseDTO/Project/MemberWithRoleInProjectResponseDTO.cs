using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Project
{
    public class MemberWithRoleInProjectResponseDTO : IdentityResponseDTO
    {
        public string FullName { get; set; }
        public RoleInTeam RoleInTeam { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
    }
}
