using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class Milestone : BaseAuditEntity<string>
{
    [ForeignKey(nameof(Project))]
    public string ProjectId { get; set; }

    [ForeignKey(nameof(Phase))]
    public string? PhaseId { get; set; }
    
    [MaxLength(50)]
    public string Title { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public Project Project { get; set; }
    public Phase? Phase { get; set; }
    public ICollection<TaskEntity>? Tasks { get; set; }

    public ICollection<MilestoneHistory>? MilestoneHistories { get; set; }
    public ICollection<Appointment>? Appointments { get; set; }
}