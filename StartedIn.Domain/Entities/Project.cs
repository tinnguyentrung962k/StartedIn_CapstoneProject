using StartedIn.Domain.Entities.BaseEntities;

namespace StartedIn.Domain.Entities;

public class Project : BaseAuditEntity<String>
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public string ProjectStatus { get; set; }   
    public ICollection<UserProject> UserProjects { get; set; }
}