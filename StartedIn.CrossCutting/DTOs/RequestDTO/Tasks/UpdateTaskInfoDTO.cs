using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Tasks
{
    public class UpdateTaskInfoDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề công việc")]
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset? Deadline { get; set; }
        public int? ManHour { get; set; }
        public string? MilestoneId { get; set; }
        public string? ParentTaskId { get; set; }
    }
}
