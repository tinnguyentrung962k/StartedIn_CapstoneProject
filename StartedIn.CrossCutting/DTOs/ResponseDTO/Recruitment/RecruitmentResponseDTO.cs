using StartedIn.CrossCutting.DTOs.BaseDTO;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Recruitment;

public class RecruitmentResponseDTO : IdentityResponseDTO
{
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string LogoUrl { get; set; }
    public bool IsOpen { get; set; }
    public string LeaderId { get; set; }
    public string LeaderName { get; set; }
    public string LeaderAvatarUrl { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset LastUpdatedTime { get; set; }
    public List<RecruitmentImgResponseDTO> RecruitmentImgs { get; set; }
}