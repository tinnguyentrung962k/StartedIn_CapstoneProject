using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class ProjectWithMembersResponseDTO
    {
        public ProjectResponseDTO Project { get; set; }
        public List<MemberWithRoleInProjectResponseDTO>? MemberWithRoleInProject { get; set; }

    }
}
