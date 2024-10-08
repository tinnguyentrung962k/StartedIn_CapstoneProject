using StartedIn.CrossCutting.Enum;

namespace StartedIn.Domain.Entities;

public class UserProject
{
    public string ProjectId { get; set; }
    public string UserId { get; set; }
    public virtual Project Project { get; set; }
    public virtual User User { get; set; }
    public RoleInTeam RoleInTeam { get; set; }
}