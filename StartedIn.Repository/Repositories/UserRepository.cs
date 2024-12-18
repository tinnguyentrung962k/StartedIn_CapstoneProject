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
                RoleInTeam = roleInTeam,
                Status = UserStatusInProject.Active
            };
            await _appDbContext.Set<UserProject>().AddAsync(userProject);
        }
        public async Task DeleteUserFromAProject(string userId, string projectId)
        {
            var userProject = await _appDbContext.Set<UserProject>()
            .Where(up => up.UserId == userId && up.ProjectId == projectId && up.Status == UserStatusInProject.Active)
            .FirstOrDefaultAsync();
            if (userProject != null) 
            {
                userProject.Status = UserStatusInProject.Left;
                await _appDbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateUserInProject(UserProject userProject)
        {
            _appDbContext.Set<UserProject>().Update(userProject);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task UpdateUserInContract(UserContract userContract)
        {
            _appDbContext.Set<UserContract>().Update(userContract);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<UserProject> GetAUserInProject(string projectId, string userId)
        {
            return await _appDbContext.Set<UserProject>().Where(x => x.ProjectId.Equals(projectId) 
            && x.UserId.Equals(userId) 
            && x.Status == UserStatusInProject.Active)
                .FirstOrDefaultAsync();
        }
        public async Task<List<UserContract>> GetUsersListRelevantToContractsInAProject(string projectId)
        {
            return await _appDbContext.Set<UserContract>().Where(x=>x.Contract.ProjectId.Equals(projectId)).Include(x=>x.User).ToListAsync();   
        }
        public async Task<bool> CheckIfUserInProject(string userId, string projectId)
        {
            var isUserInProject = await _appDbContext.UserProjects.AnyAsync(x => x.UserId.Equals(userId) 
            && x.ProjectId.Equals(projectId) 
            && x.Status == UserStatusInProject.Active);
            return isUserInProject;
        }
        public async Task<bool> IsUserBelongToAContract(string userId, string contractId)
        {
            var isUserBelongContract = await _appDbContext.UserContracts.AnyAsync(x => (x.UserId.Equals(userId) || x.TransferToId.Equals(userId))
            && x.ContractId.Equals(contractId));
            return isUserBelongContract;
        }
        public async Task<int> Count()
        {
            // TODO check deleted
            return await _appDbContext.Users.CountAsync();
        }
        public IQueryable<User> GetUsersInTheSystemQuery()
        {
            var query = _appDbContext.Users
                .Include(it => it.UserRoles)
                .ThenInclude(r => r.Role);
            return query;
        }
    }
}
