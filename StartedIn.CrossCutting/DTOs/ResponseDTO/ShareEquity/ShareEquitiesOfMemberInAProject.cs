using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.ShareEquity
{
    public class ShareEquitiesOfMemberInAProject
    {
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string Percentage { get; set; }
        public RoleInTeam StakeHolderType { get; set; }
        public DateOnly? DateAssigned { get; set; }
    }
}
