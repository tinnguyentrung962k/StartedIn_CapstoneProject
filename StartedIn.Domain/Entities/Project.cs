using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities.BaseEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartedIn.Domain.Entities;

public class Project : BaseAuditEntity<string>
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public ProjectStatus ProjectStatus { get; set; }   
    public IEnumerable<UserProject> UserProjects { get; set; }
    public IEnumerable<Phase> Phases { get; set; }
}