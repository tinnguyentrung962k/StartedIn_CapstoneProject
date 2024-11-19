using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class Phase : BaseAuditEntity<string>
{
    public string PhaseName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    
}