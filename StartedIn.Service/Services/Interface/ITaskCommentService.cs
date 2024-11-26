using StartedIn.CrossCutting.DTOs.RequestDTO.TaskComment;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface ITaskCommentService
    {
        Task<TaskComment> AddComment(string projectId, string taskId, string userId, TaskCommentCreateDTO taskCommentCreateDTO);
        Task<TaskComment> DeleteComment();
    }
}
