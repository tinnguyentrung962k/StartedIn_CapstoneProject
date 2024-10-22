using Microsoft.AspNetCore.Authorization;
using StartedIn.CrossCutting.Constants;

namespace StartedIn.API.Security
{
    public static class PoliciesConstants
    {
        public static readonly AuthorizationPolicy PolicyAdmin = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser().RequireRole(RoleConstants.ADMIN).Build();

        public static readonly AuthorizationPolicy PolicyUser = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser().RequireRole(RoleConstants.USER).Build();
    }
}
