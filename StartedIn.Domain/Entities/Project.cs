using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities.BaseEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartedIn.Domain.Entities;

public class Project : BaseAuditEntity<string>
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public string? LogoUrl { get; set; }
    public ProjectStatusEnum ProjectStatus { get; set; }
    public int? TotalShares { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal RemainingPercentOfShares { get; set; } = 100;
    public int? RemainingShares { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? CompanyIdNumber { get; set; }
    public string? HarshClientIdPayOsKey { get; set; }
    public string? HarshPayOsApiKey { get; set; }
    public string? HarshChecksumPayOsKey { get; set; }  
    public ICollection<UserProject>? UserProjects { get; set; }
    public ICollection<Milestone>? Milestones { get; set; }
    public ICollection<Contract>? Contracts { get; set; }
    public Finance Finance { get; set; }
}