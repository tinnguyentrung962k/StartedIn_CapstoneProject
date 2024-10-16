using StartedIn.Domain.Entities.BaseEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartedIn.Domain.Entities;

public class Project : BaseAuditEntity<String>
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public string ProjectStatus { get; set; }   
    public ICollection<UserProject> UserProjects { get; set; }
    public ICollection<Phase> Phases { get; set; }
}