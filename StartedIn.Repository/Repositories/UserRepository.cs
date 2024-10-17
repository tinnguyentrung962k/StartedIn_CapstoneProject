using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories
{
    public class UserRepository : UserStore<User, Role, AppDbContext>, IUserRepository
    {
        private readonly AppDbContext _appDbContext;
        public UserRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public async Task AddUserToProject(string userId, string projectId, RoleInTeam roleInTeam)
        {
            var userProject = new UserProject
            {
                UserId = userId,
                ProjectId = projectId,
                RoleInTeam = roleInTeam
            };
            await _appDbContext.Set<UserProject>().AddAsync(userProject);
        }

        public async Task<UserProject> GetAUserInProject(string projectId, string userId)
        {
            return await _appDbContext.Set<UserProject>().Where(x => x.ProjectId.Equals(projectId) && x.UserId.Equals(userId)).FirstOrDefaultAsync();
        }

    }
}
