﻿using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface IContractRepository : IGenericRepository<Contract, string>
    {
        Task<Contract> GetContractById(string contractId);
        Task<IEnumerable<Contract>> GetContractsByUserIdInAProject(string userId, string projectId, int pageIndex, int pageSize);
        Task<Contract> GetTheNearestExpiredInternalContractInAProject(string projectId);
        Task<List<Contract>> GetContractByProjectId(string projectId);
        IQueryable<Contract> GetContractListQuery(string userId, string projectId);
        Task UpdateUserInContract(UserContract userContract);
        Task<RoleInContract> GetUserRoleInContract(string userId, string contractId);
    }
}
