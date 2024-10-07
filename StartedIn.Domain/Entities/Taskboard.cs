using System.ComponentModel.DataAnnotations.Schema;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class Taskboard : BaseAuditEntity<string>
{
    [ForeignKey(nameof(Milestone))]
    public string MilestoneId { get; set; }
    [ForeignKey(nameof(Phase))]
    public string PhaseId { get; set; }
    public string Title { get; set; }
    public int Position { get; set; }
    public Milestone Milestone { get; set; }
    public Phase Phase { get; set; }
}