using Microsoft.AspNetCore.Http;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.TaskAttachment;

public class TaskAttachmentCreateDTO
{
    public string TaskId { get; set; }
    public IFormFile Attachment { get; set; }
}