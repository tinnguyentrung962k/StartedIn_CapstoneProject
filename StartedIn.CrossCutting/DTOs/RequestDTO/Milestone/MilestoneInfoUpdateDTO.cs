﻿using System.ComponentModel.DataAnnotations;
namespace StartedIn.CrossCutting.DTOs.RequestDTO.Milestone
{
    public class MilestoneInfoUpdateDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề cột mốc")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string? Description { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? PhaseId { get; set; }
    }
}
