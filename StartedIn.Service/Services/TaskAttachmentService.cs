using StartedIn.CrossCutting.DTOs.RequestDTO.TaskAttachment;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services;

public class TaskAttachmentService : ITaskAttachmentService
{
    public Task<TaskAttachment> AddTaskAttachment(TaskAttachmentCreateDTO taskAttachmentCreateDto)
    {
        throw new NotImplementedException();
    }
}