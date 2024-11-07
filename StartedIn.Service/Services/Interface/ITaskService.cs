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
        Task<TaskEntity> UpdateTaskInfo(string userId, string id, UpdateTaskInfoDTO updateTaskInfoDTO);
        Task<TaskEntity> GetTaskDetail(string userId, string taskId);
        Task<PaginationDTO<TaskResponseDTO>> GetAllTask(string userId, string projectId, int size, int page);
    }
}
