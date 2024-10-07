using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class MilestoneHistory : BaseAuditEntity<string>
{
    public string MilestoneId { get; set; }
    public string Content { get; set; }
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.Now;
}