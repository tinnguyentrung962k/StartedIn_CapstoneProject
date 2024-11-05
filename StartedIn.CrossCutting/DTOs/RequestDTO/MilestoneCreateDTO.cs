using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
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
