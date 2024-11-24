using StartedIn.CrossCutting.DTOs.RequestDTO.Project;
using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
using StartedIn.CrossCutting.DTOs.ResponseDTO.RecruitInvite;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IRecruitInviteService
    {
        Task SendJoinProjectInvitation(string userId, List<ProjectInviteEmailAndRoleDTO> inviteUsers, string projectId);
        Task AddUserToProject(string projectId, string userId, RoleInTeam roleInTeam);
        Task<ProjectInviteOverviewDTO> GetProjectInviteOverview(string projectId);
        Task AcceptProjectInvitation(string userId, string projectId, AcceptInviteDTO acceptInviteDTO);
    }
}
