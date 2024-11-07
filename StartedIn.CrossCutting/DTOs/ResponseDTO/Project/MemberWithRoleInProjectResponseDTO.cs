using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartedIn.CrossCutting.DTOs.BaseDTO;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Project
{
    public class MemberWithRoleInProjectResponseDTO : IdentityResponseDTO
    {
        public string FullName { get; set; }
        public RoleInTeam RoleInTeam { get; set; }
        public string Email { get; set; }
    }
}
