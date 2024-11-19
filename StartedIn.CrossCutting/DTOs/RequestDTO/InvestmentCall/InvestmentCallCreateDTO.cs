using System.ComponentModel.DataAnnotations;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.InvestmentCall;

public class InvestmentCallCreateDTO
{
    [Required]
    public decimal TargetCall { get; set; }
    [Required]
    public decimal EquityShareCall { get; set; }
    [Required]
    public DateOnly StartDate { get; set; }
    [Required]
    public DateOnly EndDate { get; set; }
}