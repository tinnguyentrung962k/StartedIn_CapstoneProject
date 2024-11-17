﻿using System.ComponentModel.DataAnnotations;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Tasks
{
    public class TaskCreateDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề công việc")]
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset? Deadline { get; set; }
        public string[] Assignees { get; set; }
        public string? Milestone { get; set; }
        public string? ParentTask { get; set; }
    }
}
