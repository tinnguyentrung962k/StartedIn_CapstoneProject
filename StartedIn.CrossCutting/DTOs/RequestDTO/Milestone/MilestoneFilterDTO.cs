using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Milestone
{
    public class MilestoneFilterDTO
    {
        public string? Title { get; set; }
        public string? PhaseId { get; set; }
    }
}
