using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite
{
    public class AcceptInviteDTO
    {
        public ApplicationTypeEnum Type { get; set; }
        public RoleInTeam Role { get; set; }
    }
}
