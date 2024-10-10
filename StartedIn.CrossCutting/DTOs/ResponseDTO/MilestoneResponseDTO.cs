using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class MilestoneResponseDTO : IdentityResponseDTO
    {
        public string MileStoneTitle { get; set; }
        public string Description { get; set; }
        public DateOnly MilestoneDate { get; set; }
        public int Position { get; set; }
    }
}
