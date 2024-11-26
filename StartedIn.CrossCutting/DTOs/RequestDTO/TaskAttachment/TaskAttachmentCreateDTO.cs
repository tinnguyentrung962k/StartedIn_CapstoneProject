using Microsoft.AspNetCore.Http;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.TaskAttachment;

public class TaskAttachmentCreateDTO
{
    public IFormFile Attachment { get; set; }
}