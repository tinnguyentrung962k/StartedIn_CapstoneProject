using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.RecruitInvite
{
    public class ApplicationDTO : IdentityResponseDTO
    {
        public string CandidateId { get; set; }
        public string ProjectId { get; set; }
        public ApplicationStatus Status { get; set; }
        public ApplicationTypeEnum Type { get; set; }
        public RoleInTeam Role { get; set; }
        public string? CVUrl { get; set; }
        public FullProfileDTO Candidate { get; set; }
    }
}
