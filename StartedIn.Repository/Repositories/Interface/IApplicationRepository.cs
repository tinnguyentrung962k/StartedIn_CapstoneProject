﻿using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Interface
{
    public interface IApplicationRepository : IGenericRepository<Application, string>
    {
        Task<IEnumerable<Application>> GetApplicationsWithCandidate(string projectId);
    }
}
