using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities.BaseEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartedIn.Domain.Entities;

public class Project : BaseAuditEntity<string>
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public string? LogoUrl { get; set; }
    public string ProjectStatus { get; set; }
    public int? TotalShares { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal RemainingPercentOfShares { get; set; } = 100;
    public int? RemainingShares { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? CompanyIdNumber { get; set; }

    public IEnumerable<UserProject>? UserProjects { get; set; }
    public IEnumerable<Milestone>? Milestones { get; set; }
}