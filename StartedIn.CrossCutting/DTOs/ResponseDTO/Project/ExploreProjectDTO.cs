using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.InvestmentCall;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Project;
public class ExploreProjectDTO : IdentityResponseDTO
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public string LogoUrl { get; set; }
    public string LeaderId { get; set; }
    public string LeaderFullName { get; set; }
    public string? LeaderProfilePicture { get; set; }
    public InvestmentCallResponseDTO? InvestmentCall { get; set; }

}