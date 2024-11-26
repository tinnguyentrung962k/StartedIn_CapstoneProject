using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.TaskHistory
{
    public class TaskHistoryResponseDTO : AuditResponseDTO
    {
        public string TaskId { get; set; }
        public string Content { get; set; }
    }
}
