using StartedIn.CrossCutting.DTOs.BaseDTO;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Document;

public class DocumentResponseDTO : IdentityResponseDTO
{
    public string DocumentName { get; set; }
    public string? Description { get; set; }
    public string AttachmentLink { get; set; }
    public string ProjectId { get; set; }
    public string ProjectApprovalId { get; set; }
}