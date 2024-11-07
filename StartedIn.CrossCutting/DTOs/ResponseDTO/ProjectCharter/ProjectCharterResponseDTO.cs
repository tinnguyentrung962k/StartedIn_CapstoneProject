using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone;


namespace StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectCharter
{
    public class ProjectCharterResponseDTO : IdentityResponseDTO
    {
        public string ProjectId { get; set; }
        public string? BusinessCase { get; set; }
        public string? Goal { get; set; }
        public string? Objective { get; set; }
        public string? Scope { get; set; }
        public string? Constraints { get; set; }
        public string? Assumptions { get; set; }
        public string? Deliverables { get; set; }
        public IEnumerable<MilestoneResponseDTO>? Milestones { get; set; }
    }
}
