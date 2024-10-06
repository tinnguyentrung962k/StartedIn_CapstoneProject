using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StartedIn.CrossCutting.Constants;
using StartedIn.Domain.Entities;

namespace StartedIn.Repository.Repositories.Extensions
{
    public static class UserManagementExtensions
    {
        public static async Task<User>? FindRefreshTokenAsync(this UserManager<User> userManager, string refreshToken)
        {
            return await userManager?.Users?.FirstOrDefaultAsync(u => u.RefreshToken.Equals(refreshToken));
        }
        public static async Task<IEnumerable<User>> GetUsersAsync(this UserManager<User> userManager, int pageIndex = 1, int pageSize = 1)
        {
            var userList = await userManager?.Users.Include(it => it.UserRoles)
                .ThenInclude(r => r.Role).ToListAsync();
            pageIndex = pageIndex < 1 ? 0 : pageIndex - 1;
            pageSize = pageSize < 1 ? 10 : pageSize;
            var pagedUsers = userList.Skip(pageIndex * pageSize).Take(pageSize);
            return pagedUsers;
        }
    }
}
