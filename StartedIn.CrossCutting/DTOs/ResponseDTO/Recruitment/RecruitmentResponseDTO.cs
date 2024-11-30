namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Recruitment;

public class RecruitmentResponseDTO
{
    public string ProjectId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public bool IsOpen { get; set; }
    public RecruitmentImgResponseDTO RecruitmentImgs { get; set; }
}