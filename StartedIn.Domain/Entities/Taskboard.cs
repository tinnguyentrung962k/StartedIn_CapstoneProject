using System.ComponentModel.DataAnnotations.Schema;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class Taskboard : BaseAuditEntity<string>
{
    [ForeignKey(nameof(Milestone))]
    public string MilestoneId { get; set; }
    public string Title { get; set; }
    public int Position { get; set; }
    public Milestone Milestone { get; set; }
    public ICollection<TaskEntity> TasksList { get; set; }
}