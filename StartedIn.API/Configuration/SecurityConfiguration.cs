using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using System.Security.Claims;
using StartedIn.API.Hubs;

namespace StartedIn.API.Configuration
{
    public static class SecurityConfiguration
    {
        public static IServiceCollection AddSecurityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.ClaimsIdentity.UserNameClaimType = ClaimTypes.NameIdentifier;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddUserStore<UserStore<User, Role, AppDbContext, string, IdentityUserClaim<string>,
                UserRole, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>>()
                .AddRoleStore<RoleStore<Role, AppDbContext, string, UserRole, IdentityRoleClaim<string>>
                >()
                .AddDefaultTokenProviders();
            return services;
        }
        public static IApplicationBuilder UseSecurityConfiguration(this IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseCors(options =>
            {
                options.AllowAnyOrigin();
                options.AllowAnyMethod();
                options.AllowAnyHeader();
            });
            app.UseAuthentication();
            app.UseAuthorization(); 
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ProjectHub>("/project");
                endpoints.MapControllers();
            });

            return app;
        }
    }
}
