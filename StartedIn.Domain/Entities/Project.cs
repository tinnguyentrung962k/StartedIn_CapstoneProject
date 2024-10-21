using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities.BaseEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartedIn.Domain.Entities;

public class Project : BaseAuditEntity<string>
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public string LogoUrl { get; set; }
    public ProjectStatus ProjectStatus { get; set; }
    public int TotalShares { get; set; }
    public decimal RemainingPercentOfShares { get; set; }
    public int RemainingShares { get; set; }
    public IEnumerable<UserProject> UserProjects { get; set; }
    public IEnumerable<Milestone> Milestones { get; set; }
}