using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class PhaseDetailResponseDTO : IdentityResponseDTO
    {
        public string ProjectId { get; set; }
        public string PhaseName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Duration { get; set; }
        public List<MilestoneResponseDTO> Milestones { get; set; }
    }
}
