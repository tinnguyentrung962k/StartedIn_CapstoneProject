using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Project;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TaskComment;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks
{
    public class TaskDetailDTO : AuditResponseDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public bool IsLate { get; set; }
        public TaskEntityStatus Status { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public int ManHour { get; set; }
        public TaskResponseDTO ParentTask { get; set; }
        public MilestoneResponseDTO Milestone { get; set; }
        public ICollection<TaskResponseDTO> SubTasks { get; set; }
        public ICollection<MemberWithRoleInProjectResponseDTO> Assignees { get; set; }
        public ICollection<TaskCommentDTO> TaskComments { get; set; }
    }
}
