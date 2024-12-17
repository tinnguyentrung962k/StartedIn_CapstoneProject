using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.RequestDTO.Project;
using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
using StartedIn.CrossCutting.DTOs.ResponseDTO.RecruitInvite;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IRecruitInviteService
    {
        Task SendJoinProjectInvitation(string userId, List<string> inviteUserEmails, string projectId);
        Task AddUserToProject(string projectId, string userId, RoleInTeam roleInTeam);
        Task<ProjectInviteOverviewDTO> GetProjectInviteOverview(string projectId);
        Task AcceptProjectInvitation(string userId, string projectId, AcceptInviteDTO acceptInviteDTO);
        Task<Application> ApplyRecruitment(string userId, string projectId, string recruitmentId, List<IFormFile> files);

        //TODO: make it pagination someday
        Task<IEnumerable<Application>> GetApplicationsOfProject(string userId, string projectId);

        Task AcceptApplication(string userId, string projectId, string applicationId);
        Task RejectApplication(string userId, string projectId, string applicationId);
    }
}
