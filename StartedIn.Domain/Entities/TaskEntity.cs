using System.ComponentModel.DataAnnotations.Schema;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class TaskEntity : BaseAuditEntity<String>
{
    [ForeignKey(nameof(Taskboard))]
    public string TaskboardId { get; set; }
    [ForeignKey(nameof(Milestone))]
    public string MilestoneId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public Taskboard Taskboard { get; set; }
    public Milestone Milestone { get; set; }
}