using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.TaskComment
{
    public class TaskCommentDTO : AuditResponseDTO
    {
        public string TaskId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public HeaderProfileDTO User { get; set; }
    }
}
