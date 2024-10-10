using StartedIn.Domain.Entities.BaseEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartedIn.Domain.Entities;

public class MilestoneHistory : BaseAuditEntity<string>
{
    [ForeignKey(nameof(Milestone))]
    public string MilestoneId { get; set; }
    public string Content { get; set; }
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.Now;
    public Milestone Milestone { get; set; }
}