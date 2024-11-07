using StartedIn.CrossCutting.Enum;
using System.ComponentModel.DataAnnotations;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Milestone
{
    public class MilestoneCreateDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tên cột mốc")]
        public string MilstoneTitle { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày đáo hạn")]
        public DateOnly DueDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giai đoạn của dự án")]
        public PhaseEnum PhaseEnum { get; set; }
    }
}
