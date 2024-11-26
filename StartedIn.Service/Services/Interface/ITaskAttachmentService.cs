using StartedIn.CrossCutting.DTOs.RequestDTO.TaskAttachment;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface ITaskAttachmentService
{
    Task<TaskAttachment> AddTaskAttachment(string taskId, string projectId, TaskAttachmentCreateDTO taskAttachmentCreateDto);
    Task DeleteTaskAttachment(string taskAttachmentId);
}