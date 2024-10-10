using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class TaskboardResponseDTO : IdentityResponseDTO
    {
        public string Title { get; set; }
        public string MilestoneId { get; set; }
        public int Position { get; set; }
        public List<TaskInTaskboardResponseDTO> TasksList { get; set; }
    }
}
