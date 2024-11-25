using StartedIn.CrossCutting.DTOs.RequestDTO.Tasks;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface ITaskService
    {
        Task<TaskEntity> CreateTask(TaskCreateDTO taskCreateDto, string userId, string projectId);
        Task<TaskEntity> UpdateTaskInfo(string userId, string taskId, string projectId, UpdateTaskInfoDTO updateTaskInfoDTO);
        Task<TaskEntity> UpdateTaskStatus(string userId, string taskId, string projectId, UpdateTaskStatusDTO updateTaskStatusDTO);
        Task<TaskEntity> UpdateTaskAssignment(string userId, string taskId, string projectId, UpdateTaskAssignmentDTO updateTaskAssignmentDTO);
        Task<TaskEntity> UpdateTaskUnassignment(string userId, string taskId, string projectId, UpdateTaskAssignmentDTO updateTaskAssignmentDTO);
        Task<TaskEntity> UpdateTaskMilestone(string userId, string taskId, string projectId, UpdateTaskMilestoneDTO updateTaskMilestoneDTO);
        Task<TaskEntity> UpdateParentTask(string userId, string taskId, string projectId, UpdateParentTaskDTO updateParentTaskDTO);
        Task<TaskEntity> GetTaskDetail(string userId, string taskId, string projectId);
        Task<PaginationDTO<TaskResponseDTO>> FilterTask(string userId, string projectId, TaskFilterDTO taskFilterDto, int size, int page);
        Task DeleteTask(string userId, string taskId, string projectId);
        Task MarkTaskAsLate();
    }
}
