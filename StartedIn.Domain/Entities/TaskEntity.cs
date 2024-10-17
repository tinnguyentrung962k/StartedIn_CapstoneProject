using System.ComponentModel.DataAnnotations.Schema;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class TaskEntity : BaseAuditEntity<string>
{
    [ForeignKey(nameof(Taskboard))]
    public string TaskboardId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskEntityStatus Status { get; set; }
    public int Position { get; set; }
    public DateTimeOffset? Deadline { get; set; }
    public Taskboard Taskboard { get; set; }
}