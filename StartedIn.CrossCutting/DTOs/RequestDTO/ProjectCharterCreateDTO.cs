using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class ProjectCharterCreateDTO
    {
        public string ProjectId { get; set; }
        public string? BusinessCase { get; set; }
        public string? Goal { get; set; }
        public string? Objective { get; set; }
        public string? Scope { get; set; }
        public string? Constraints { get; set; }
        public string? Assumptions { get; set; }
        public string? Deliverables { get; set; }
        public List<MilestoneInCharterCreateDTO> ListMilestoneCreateDto { get; set; }
    }
}
