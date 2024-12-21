using Microsoft.AspNetCore.Http;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.ProjectApproval;

public class CreateProjectApprovalDTO
{
    public string Reason { get; set; }
    public List<IFormFile> Documents { get; set; }
}