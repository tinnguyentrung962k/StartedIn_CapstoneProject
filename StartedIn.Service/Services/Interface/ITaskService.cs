using StartedIn.CrossCutting.DTOs.RequestDTO;
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
        Task<TaskEntity> CreateTask(TaskCreateDTO taskCreateDto, string userId);
        Task<TaskEntity> UpdateTaskInfo(string userId, string id, UpdateTaskInfoDTO updateTaskInfoDTO);
        Task<TaskEntity> GetATaskDetail(string userId, string taskId);
    }
}
