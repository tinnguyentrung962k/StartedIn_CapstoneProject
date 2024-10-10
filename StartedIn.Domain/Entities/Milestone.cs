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
    public int Position { get; set; }
    public Phase Phase { get; set; }
    public ICollection<Taskboard> Taskboards { get; set; }
}