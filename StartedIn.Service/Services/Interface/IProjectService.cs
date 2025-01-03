using StartedIn.CrossCutting.DTOs.RequestDTO.Project;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Project;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IProjectService 
{
    Task<Project> CreateNewProject(string userId, ProjectCreateDTO projectCreateDTO);
    Task<Project> GetProjectById(string id);
    Task<Project> GetProjectAndMemberById(string id);
    Task<List<ProjectResponseDTO>> GetListParticipatedProjects(string userId);
    Task<List<User>> GetListUserRelevantToContractsInAProject(string projectId);
    Task<PaginationDTO<ExploreProjectDTO>> GetProjectsForInvestor(string userId, ProjectFilterDTO projectFilterDTO, int size, int page);
    Task AddPaymentGatewayInfo(string userId, string projectId, PayOsPaymentGatewayRegisterDTO payOsPaymentGatewayRegisterDTO);
    Task<PayOsPaymentGatewayResponseDTO> GetPaymentGatewayInfoByProjectId(string userId, string projectId);
    Task<PaginationDTO<ProjectResponseDTO>> GetAllProjectsForAdmin(ProjectAdminFilterDTO projectAdminFilterDTO,int page, int size);
    Task<Project> ActivateProject(string projectId);
    Task<ProjectDashboardDTO> GetProjectDashboard(string userId, string projectId);
    Task CloseAProject(string userId, string projectId);
    Task<ClosingProjectInformationDTO> GetProjectClosingInformation(string userId, string projectId);
    Task<LeavingProjectInfomationDTO> GetProjectLeavingInformation(string userId, string projectId);
    Task<ProjectInformationWithMembersResponseDTO> GetProjectInformationWithMemberById(string projectId);
    Task<List<ProjectResponseDTO>> GetProjectsThatUserLeft(string userId);
    Task<List<ProjectResponseDTO>> GetClosedProjectsForUser(string userId);
    Task UpdateProjectDetail(string userId, string projectId, ProjectDetailPostDTO projectDetail);
    Task AddAppointmentUrl(string userId, string projectId, AppointmentUrlDTO appointmentUrlDto);
}