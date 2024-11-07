using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class MilestoneAndTaskResponseDTO
    {
        public MilestoneResponseDTO Milestone { get; set; }
        public List<TaskResponseDTO> Tasks { get; set; }
    }
}
