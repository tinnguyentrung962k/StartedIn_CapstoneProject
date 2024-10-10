using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class TaskInTaskboardResponseDTO : IdentityResponseDTO
    {
        public int Position { get; set; }
        public string TaskTitle { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string TaskboardId { get; set; }
        public DateOnly? Deadline { get; set; }
    }
}
