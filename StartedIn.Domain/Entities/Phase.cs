using System.ComponentModel.DataAnnotations.Schema;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class Phase : BaseAuditEntity<string>
{
    public string PhaseName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    [ForeignKey(nameof(ProjectCharter))]
    public string ProjectCharterId { get; set; }
    public ProjectCharter ProjectCharter { get; set; }
    
    public ICollection<Milestone>? Milestones { get; set; }
}