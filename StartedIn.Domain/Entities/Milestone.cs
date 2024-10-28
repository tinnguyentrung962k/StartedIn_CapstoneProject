using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class Milestone : BaseAuditEntity<string>
{
    [ForeignKey(nameof(Project))]
    public string ProjectId { get; set; }

    [ForeignKey(nameof(ProjectCharter))]
    public string? CharterId { get; set; }
    
    [MaxLength(50)]
    public string PhaseName { get; set; }
    
    [MaxLength(50)]
    public string Title { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly? ExtendedDate { get; set; }
    public int? ExtendedCount { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Percentage { get; set; }
    public Project Project { get; set; }
    public ICollection<TaskEntity>? Tasks { get; set; }
    [JsonIgnore] public ProjectCharter ProjectCharter { get; set; }
    public ICollection<MilestoneHistory>? MilestoneHistories { get; set; }
}