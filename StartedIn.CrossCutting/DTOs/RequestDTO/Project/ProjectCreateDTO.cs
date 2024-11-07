using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;


namespace StartedIn.CrossCutting.DTOs.RequestDTO.Project
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
