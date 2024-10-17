using System.ComponentModel.DataAnnotations.Schema;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class Phase : BaseAuditEntity<string>
{
    [ForeignKey(nameof(Project))]
    public string ProjectId { get; set; }
    public string PhaseName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Duration { get; set; }
    
    public Project Project { get; set; }
    public ICollection<Milestone> Milestones { get; set; }
}