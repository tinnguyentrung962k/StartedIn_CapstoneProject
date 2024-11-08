using Microsoft.AspNetCore.Mvc;
using StartedIn.API.Filters;

namespace StartedIn.API.Attributes
{
    public class RequireProjectAccessAttribute : TypeFilterAttribute
    {
        public RequireProjectAccessAttribute() : base(typeof(ProjectAccessAuthorizationFilter))
        {
        }
    }
}
