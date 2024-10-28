using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IProjectCharterService
    {
        Task<ProjectCharter> CreateNewProjectCharter(string userId, ProjectCharterCreateDTO projectCharter);

        Task<ProjectCharter> GetProjectCharterByCharterId(string id);
        Task<ProjectCharter> GetProjectCharterByProjectId(string projectId);
    }
}
