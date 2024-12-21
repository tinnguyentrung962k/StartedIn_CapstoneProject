using System.ComponentModel.DataAnnotations.Schema;
using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class ApplicationFile : BaseAuditEntity<string>
{
    [ForeignKey(nameof(Application))]
    public string ApplicationId { get; set; }
    public string FileName { get; set; }
    public string FileUrl { get; set; }
}