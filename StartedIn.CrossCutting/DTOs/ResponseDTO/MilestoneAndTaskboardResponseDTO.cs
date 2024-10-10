using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class MilestoneAndTaskboardResponseDTO
    {
        public MilestoneResponseDTO Milestone { get; set; }
        public List<TaskboardResponseDTO> Taskboards { get; set; }
    }
}
