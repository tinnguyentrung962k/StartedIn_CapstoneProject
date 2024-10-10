using StartedIn.CrossCutting.DTOs.RequestDTO;
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
        Task<TaskEntity> CreateTask(TaskCreateDTO taskCreateDto);
        Task<TaskEntity> MoveTask(string taskId, string taskBoardId, int position, bool needsReposition);
        Task<TaskEntity> UpdateTaskInfo(string id, UpdateTaskInfoDTO updateTaskInfoDTO);
    }
}
