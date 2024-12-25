using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone
{
    public class MilestoneHistoryResponseDTO : AuditResponseDTO
    {
        public string MilestoneId { get; set; }
        public string Content { get; set; }
    }
}
