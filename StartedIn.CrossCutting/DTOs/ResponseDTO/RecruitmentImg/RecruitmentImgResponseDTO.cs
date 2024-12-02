using StartedIn.CrossCutting.DTOs.BaseDTO;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO;

public class RecruitmentImgResponseDTO : IdentityResponseDTO
{
    public string FileName { get; set; }
    public string ImageUrl { get; set; }
}