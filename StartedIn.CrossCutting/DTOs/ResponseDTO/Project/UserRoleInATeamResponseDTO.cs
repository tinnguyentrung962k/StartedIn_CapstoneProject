using Newtonsoft.Json;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Project
{
    public class UserRoleInATeamResponseDTO
    {
        [JsonProperty("roleInTeam")]
        public RoleInTeam RoleInTeam { get; set; }
    }
}
