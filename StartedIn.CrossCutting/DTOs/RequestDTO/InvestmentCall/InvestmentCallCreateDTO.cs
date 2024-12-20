using System.ComponentModel.DataAnnotations;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.InvestmentCall;

public class InvestmentCallCreateDTO
{
    [Required]
    [Range(0, float.MaxValue)]
    public decimal TargetCall { get; set; }
    [Required]
    [Range(0,100)]
    public decimal EquityShareCall { get; set; }
    [Required]
    [Range(0, float.MaxValue)]
    public decimal ValuePerPercentage { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }
}