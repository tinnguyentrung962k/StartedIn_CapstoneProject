using StartedIn.CrossCutting.DTOs.BaseDTO;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.TaskAttachment;

public class TaskAttachmentResponseDTO : IdentityResponseDTO
{
    public string TaskId { get; set; }
    public string AttachmentUrl { get; set; }
    public string FileName { get; set; }
}