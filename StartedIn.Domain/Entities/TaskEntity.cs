using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class TaskEntity : BaseAuditEntity<string>
{
    [ForeignKey(nameof(Milestone))]
    public string MilestoneId { get; set; }

    [MaxLength(50)]
    public string Title { get; set; }
    [MaxLength(255)]
    public string? Description { get; set; }
    [MaxLength(10)]
    public string Status { get; set; }
    public DateTimeOffset? Deadline { get; set; }
    public Milestone Milestone { get; set; }
    public ICollection<TaskComment> TaskComments { get; set; }
    public ICollection<TaskHistory> TaskHistories { get; set; }
    public ICollection<TaskAttachment> TaskAttachments { get; set; }
    
}