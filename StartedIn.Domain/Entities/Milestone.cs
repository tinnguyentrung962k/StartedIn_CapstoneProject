using System.ComponentModel.DataAnnotations.Schema;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class Milestone : BaseAuditEntity<string>
{
    [ForeignKey(nameof(Phase))]
    public string PhaseId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateOnly MilestoneDate { get; set; }
    public DateOnly? ExtendedDate { get; set; }
    public int? ExtendedCount { get; set; }
    public decimal? Percentage { get; set; }
    public Phase Phase { get; set; }
    public ICollection<Taskboard> Taskboards { get; set; }
}