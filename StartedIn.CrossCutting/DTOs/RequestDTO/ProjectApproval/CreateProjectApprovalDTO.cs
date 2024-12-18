using Microsoft.AspNetCore.Http;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.ProjectApproval;

public class CreateProjectApprovalDTO
{
    public List<IFormFile> Documents { get; set; }
}