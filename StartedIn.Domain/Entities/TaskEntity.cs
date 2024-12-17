using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class TaskEntity : BaseAuditEntity<string>
{
    [ForeignKey(nameof(Milestone))]
    public string? MilestoneId { get; set; }
    [ForeignKey(nameof(Project))]
    public string ProjectId { get; set; }
    public string? ParentTaskId { get; set; }
    [MaxLength(50)]
    public string Title { get; set; }
    [MaxLength(255)]
    public string? Description { get; set; }
    [MaxLength(18)]
    public TaskEntityStatus Status { get; set; }
    public bool IsLate { get; set; }
    public int ManHour { get; set; } = 0;
    public DateTimeOffset? ActualFinishAt { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public Milestone Milestone { get; set; }
    public Project Project { get; set; }
    public TaskEntity ParentTask { get; set; }
    public ICollection<UserTask> UserTasks { get; set; } = [];
    public ICollection<TaskEntity> SubTasks { get; set; } = [];
    public ICollection<TaskComment> TaskComments { get; set; } = [];
    public ICollection<TaskHistory> TaskHistories { get; set; } = [];
    public ICollection<TaskAttachment> TaskAttachments { get; set; } = [];

}