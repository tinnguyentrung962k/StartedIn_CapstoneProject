﻿using Microsoft.AspNetCore.Identity;
using Serilog;
using StartedIn.CrossCutting.Constants;
using StartedIn.Domain.Entities;

namespace StartedIn.API.Configuration
{
    public static class IdentityStartup
    {
        public static IApplicationBuilder SeedIdentity(this IApplicationBuilder builder)
        {
            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

                SeedRoles(roleManager).Wait();
                SeedUsers(userManager).Wait();
                SeedUserRoles(userManager).Wait();
            }

            return builder;
        }
        private static IEnumerable<Role> Roles()
        {
            return new List<Role>
            {
                new Role {Id = "role_admin", Name = RoleConstants.ADMIN},
                new Role {Id = "role_user",Name = RoleConstants.USER},
                new Role {Id = "role_investor",Name = RoleConstants.INVESTOR},
                new Role {Id = "role_mentor",Name = RoleConstants.MENTOR}
            };
        }

        private static IEnumerable<User> Users()
        {
            return new List<User>
            {

                new User
                {
                    Id = "admin",
                    UserName = "admin@gmail.com",
                    FullName = "Administrator",
                    PasswordHash = "AQAAAAIAAYagAAAAEDVvGpkikGvRZ56Ri2MKtaJTlb+tqMqrUG0TM7irCuj430fot1Qiq5eopSnTR+vbew==",
                    ProfilePicture = ProfileConstant.defaultAvatarUrl,
                    Email = "admin@gmail.com",
                    EmailConfirmed = true,
                    IsActive = true
                },
                new User
                {
                    Id = "user-demo",
                    UserName = "user@gmail.com",
                    FullName = "User Demo",
                    PasswordHash = "AQAAAAIAAYagAAAAEDVvGpkikGvRZ56Ri2MKtaJTlb+tqMqrUG0TM7irCuj430fot1Qiq5eopSnTR+vbew==",
                    ProfilePicture = ProfileConstant.defaultAvatarUrl,
                    Email = "user@gmail.com",
                    EmailConfirmed = true,
                    IsActive = true
                },
            };
        }

        private static IDictionary<string, string[]> UserRoles()
        {
            return new Dictionary<string, string[]>
            {
                { "admin", new[] {RoleConstants.ADMIN}},
                { "user-demo", new[] {RoleConstants.USER }}
            };
        }


        private static async Task SeedRoles(RoleManager<Role> roleManager)
        {
            foreach (var role in Roles())
            {
                var dbRole = await roleManager.FindByNameAsync(role.Name);
                if (dbRole == null)
                {
                    try
                    {
                        await roleManager.CreateAsync(role);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Failed to create role {role}", role.Name);
                    }
                }
                else
                {
                    await roleManager.UpdateAsync(dbRole);
                }
            }
        }

        private static async Task SeedUsers(UserManager<User> userManager)
        {
            foreach (var user in Users())
            {
                var dbUser = await userManager.FindByIdAsync(user.Id);
                if (dbUser == null)
                {
                    try
                    {
                        await userManager.CreateAsync(user);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Failed to create user {user}", user.Email);
                    }
                }
                else
                {
                    await userManager.UpdateAsync(dbUser);
                }
            }
        }

        private static async Task SeedUserRoles(UserManager<User> userManager)
        {
            foreach (var (id, roles) in UserRoles())
            {
                try
                {
                    var user = await userManager.FindByIdAsync(id);
                    await userManager.AddToRolesAsync(user, roles);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to assign roles to user {userId}", id);
                }
            }
        }
    }
}
