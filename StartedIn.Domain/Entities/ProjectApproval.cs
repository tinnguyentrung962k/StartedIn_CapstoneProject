using System.ComponentModel.DataAnnotations.Schema;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class ProjectApproval : BaseAuditEntity<string>
{
    [ForeignKey(nameof(Project))]
    public string ProjectId { get; set; }
    public Project Project { get; set; }
    public ProjectApprovalStatus Status { get; set; }
    public string? RejectReason { get; set; }
    public ICollection<Document> Documents { get; set; }
}