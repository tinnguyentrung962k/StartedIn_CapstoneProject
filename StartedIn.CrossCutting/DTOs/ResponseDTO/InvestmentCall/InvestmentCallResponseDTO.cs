using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.InvestmentCall;

public class InvestmentCallResponseDTO : IdentityResponseDTO
{
    public string ProjectId { get; set; }
    public string TargetCall { get; set; }
    public string AmountRaised { get; set; }
    public string RemainAvailableEquityShare { get; set; }
    public string EquityShareCall { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public InvestmentCallStatus Status { get; set; }
    public int TotalInvestor { get; set; }
}