using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.ProjectApproval;

public class CreateProjectApprovalDTO
{
    public string Reason { get; set; }
    public List<IFormFile> Documents { get; set; }
    
    [Required]
    [Range(0, 100)]
    public decimal EquityShareCall { get; set; }
    [Required]
    [Range(0, float.MaxValue)]
    public decimal ValuePerPercentage { get; set; }
    public DateOnly? EndDate { get; set; }
}