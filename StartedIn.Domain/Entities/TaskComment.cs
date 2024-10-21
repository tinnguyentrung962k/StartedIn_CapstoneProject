using System.ComponentModel.DataAnnotations.Schema;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class TaskComment : BaseAuditEntity<string>
{
    [ForeignKey(nameof(TaskEntity))]
    public string TaskId { get; set; }
    public string Content { get; set; }
    
    public TaskEntity TaskEntity { get; set; }
}