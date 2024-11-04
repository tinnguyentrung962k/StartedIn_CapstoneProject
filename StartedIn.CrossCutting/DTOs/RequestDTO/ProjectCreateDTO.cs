using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class ProjectCreateDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tên dự án")]
        public string ProjectName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả dự án")]
        public string Description { get; set; }
        [Required]
        public IFormFile? LogoFile { get; set; }
        public int? TotalShares { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
}
