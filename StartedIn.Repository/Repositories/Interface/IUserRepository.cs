using Microsoft.AspNetCore.Identity;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface IUserRepository : IUserStore<User>
    {
        Task AddUserToProject(string userId, string projectId, RoleInTeam roleInTeam);
        Task<UserProject> GetAUserInProject(string projectId, string userId);
        Task<List<UserContract>> GetUsersListRelevantToContractsInAProject(string projectId);
        Task<bool> CheckIfUserInProject(string userId, string projectId);
        Task<bool> IsUserBelongToAContract(string userId, string contractId);

    }
}
