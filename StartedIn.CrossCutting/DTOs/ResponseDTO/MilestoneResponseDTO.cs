using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class MilestoneResponseDTO : IdentityResponseDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateOnly MilestoneDate { get; set; }
        public DateOnly? ExtendedDate { get; set; }
        public int? ExtendedCount { get; set; }
    }
}
