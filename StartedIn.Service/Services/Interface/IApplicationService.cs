using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IApplicationService
{
    Task<Application> CreateApplication(CreateApplicationDTO createApplicationDto, string userId, string projectId);
}