using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class MemberWithRoleInProjectResponseDTO : IdentityResponseDTO
    {
        public string FullName { get; set; }
        public RoleInTeam RoleInTeam { get; set; }
        public string Email { get; set; }
    }
}
