using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone
{
    public class MilestoneProgressResponseDTO : IdentityResponseDTO
    {
        public string? Title { get; set; }
        public int? Progress { get; set; }
    }
}
