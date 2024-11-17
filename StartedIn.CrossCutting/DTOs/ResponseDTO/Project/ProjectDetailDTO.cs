using StartedIn.CrossCutting.DTOs.ResponseDTO.InvestmentCall;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectCharter;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Project;

public class ProjectDetailDTO : ProjectResponseDTO
{
    public InvestmentCallResponseDTO InvestmentCallResponseDto { get; set; }
    public ProjectCharterResponseDTO ProjectCharterResponseDto { get; set; }
}