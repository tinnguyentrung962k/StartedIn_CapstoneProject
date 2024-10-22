using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class TaskCreateDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề công việc")]
        public string TaskTitle { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn cột mốc")]
        public string MilestoneId { get; set; }
        public DateTimeOffset? Deadline { get; set; }
    }
}
