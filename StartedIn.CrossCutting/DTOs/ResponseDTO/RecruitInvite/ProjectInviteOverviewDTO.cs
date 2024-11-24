using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.RecruitInvite
{
    public class ProjectInviteOverviewDTO : AuditResponseDTO
    {
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public string? LogoUrl { get; set; }
        public ProjectStatusEnum ProjectStatus { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public ProjectInviteUserDTO Leader { get; set; }

    }
}
