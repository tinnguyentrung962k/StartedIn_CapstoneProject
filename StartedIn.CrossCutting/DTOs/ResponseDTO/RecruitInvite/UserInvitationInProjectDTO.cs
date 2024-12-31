using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.RecruitInvite
{
    public class UserInvitationInProjectDTO : IdentityResponseDTO
    {
        public string CandidateId { get; set; }
        public string CandidateEmail { get; set; }
        public string CandidateName { get; set; }
        public string CandidatePhoneNumber { get; set; }
        public ApplicationStatus Status { get; set; }
        public ApplicationTypeEnum Type { get; set; }
        public RoleInTeam Role { get; set; }
    }
}
