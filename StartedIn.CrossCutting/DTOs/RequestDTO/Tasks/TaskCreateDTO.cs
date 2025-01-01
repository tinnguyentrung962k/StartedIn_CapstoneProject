using System.ComponentModel.DataAnnotations;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Tasks
{
    public class TaskCreateDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề công việc")]
        public string Title { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ngày bắt đầu công việc")]
        public DateTimeOffset? StartDate { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ngày dự kiến kết thúc công việc")]
        public DateTimeOffset? EndDate { get; set; }
        public string[] Assignees { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập dự kiến thời gian hoàn thành công việc")]
        public int? ManHour { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập độ ưu tiên của công việc")]
        public int? Priority { get; set; }
        public string? Milestone { get; set; }
        public string? ParentTask { get; set; }
    }
}
