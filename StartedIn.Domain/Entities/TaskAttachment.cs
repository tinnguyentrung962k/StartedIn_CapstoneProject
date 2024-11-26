using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class TaskAttachment : BaseAuditEntity<string>
{
    [ForeignKey(nameof(TaskEntity))]
    public string TaskId { get; set; }
    public string FileName { get; set; }
    public string? AttachmentUrl { get; set; }
    public TaskEntity TaskEntity { get; set; }
}