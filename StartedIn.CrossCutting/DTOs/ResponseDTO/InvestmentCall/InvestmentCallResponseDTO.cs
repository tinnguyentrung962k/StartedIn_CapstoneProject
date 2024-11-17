using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.InvestmentCall;

public class InvestmentCallResponseDTO : IdentityResponseDTO
{
    public string ProjectId { get; set; }
    public decimal TargetCall { get; set; }
    public decimal AmountRaised { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public InvestmentCallStatus Status { get; set; }
    public int TotalInvestor { get; set; }
}