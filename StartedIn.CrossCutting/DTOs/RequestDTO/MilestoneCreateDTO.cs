using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class MilestoneCreateDTO
    {
        public string PhaseId { get; set; }
        public string MilstoneTitle { get; set; }
        public string Description { get; set; }
        public DateOnly MilestoneDate { get; set; }
        public int Position { get; set; }
    }
}
