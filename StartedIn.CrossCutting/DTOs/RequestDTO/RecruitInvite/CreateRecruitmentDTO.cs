using Microsoft.AspNetCore.Http;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;

public class CreateRecruitmentDTO
{
    public string Title { get; set; }
    public string Content { get; set; }
    public List<IFormFile> recruitFiles { get; set; } 
}