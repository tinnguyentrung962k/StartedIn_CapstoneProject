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
        public string RoleInTeam { get; set; }
    }
}
