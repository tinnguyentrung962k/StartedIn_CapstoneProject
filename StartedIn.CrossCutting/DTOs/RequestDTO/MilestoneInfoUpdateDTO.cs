using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class MilestoneInfoUpdateDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề cột mốc")]
        public string MilestoneTitle { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string? Description { get; set; }
        public DateOnly DueDate { get; set; }
    }
}
