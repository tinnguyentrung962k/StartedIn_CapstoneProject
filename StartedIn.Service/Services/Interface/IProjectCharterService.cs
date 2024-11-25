using StartedIn.CrossCutting.DTOs.RequestDTO.ProjectCharter;
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
        Task<ProjectCharter> CreateNewProjectCharter(string userId, string projectId, ProjectCharterCreateDTO projectCharter);
        
        Task<ProjectCharter> GetProjectCharterByProjectId(string projectId);

        Task<ProjectCharter> UpdateProjectCharterInfo(string userId, string projectId,
            EditProjectCharterDTO editProjectCharterDto);
    }
}
