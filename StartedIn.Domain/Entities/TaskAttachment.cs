using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartedIn.Domain.Entities;

public class TaskAttachment 
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } 
    [ForeignKey(nameof(TaskEntity))]
    public string TaskId { get; set; }
    public TaskEntity TaskEntity { get; set; }
}