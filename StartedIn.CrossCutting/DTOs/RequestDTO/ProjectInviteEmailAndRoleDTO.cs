using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class ProjectInviteEmailAndRoleDTO
    {
        public string Email { get; set; }
        public RoleInTeam RoleInTeam { get; set; }
    }
}
