using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class MilestoneCreateDTO
    {
        public string ProjectId { get; set; }
        public string MilstoneTitle { get; set; }
        public string Description { get; set; }
        public DateOnly MilestoneDate { get; set; }
        public PhaseEnum PhaseEnum { get; set; }
    }
}
