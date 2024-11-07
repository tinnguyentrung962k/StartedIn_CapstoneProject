using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks
{
    public class TaskDetailDTO : AuditResponseDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public bool IsLate { get; set; }
        public TaskEntityStatus Status { get; set; }
        public DateTimeOffset? Deadline { get; set; }
    }
}
