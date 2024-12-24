using AutoMapper;
using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.Project;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Contract;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Project;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services;
using StartedIn.Service.Services.Interface;
using System.Diagnostics.Contracts;
using System.Security.Claims;

namespace StartedIn.API.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ILogger<ProjectController> _logger;
    private readonly IProjectService _projectService;
    private readonly IUserService _userService;

    public ProjectController(IProjectService projectService, IMapper mapper, ILogger<ProjectController> logger, IUserService userService)
    {
        _projectService = projectService;
        _mapper = mapper;
        _logger = logger;
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
    public async Task<ActionResult<List<ProjectResponseDTO>>> GetListOfProjectsWithRole()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        try
        {
            var participatedProjects = await _projectService.GetListParticipatedProjects(userId);
            return Ok(participatedProjects);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpGet("closed-projects")]
    [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
    public async Task<ActionResult<ClosedProjectsOrUserLeftProjectsDTO>> GetClosedProjectsOrProjectsThatUserLeft()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        try
        {
            var closedProjects = await _projectService.GetClosedProjectsForUser(userId);
            var leftProjects = await _projectService.GetProjectsThatUserLeft(userId);
            var response = new ClosedProjectsOrUserLeftProjectsDTO
            {
                ClosedProjects = closedProjects,
                UserLeftProjects = leftProjects
            };
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<ProjectResponseDTO>> CreateANewProject([FromForm] ProjectCreateDTO projectCreatedto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var newProject = await _projectService.CreateNewProject(userId, projectCreatedto);
            var responseNewProject = _mapper.Map<ProjectResponseDTO>(newProject);
            return CreatedAtAction(nameof(GetProjectById), new { projectId = responseNewProject.Id }, responseNewProject);
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ExistedRecordException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(MessageConstant.CreateFailed);
        }
    }

    [HttpGet("{projectId}")]
    [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
    public async Task<ActionResult<ProjectDetailDTO>> GetProjectById(string projectId)
    {
        try
        {
            var project = await _projectService.GetProjectById(projectId);
            var mappedProject = _mapper.Map<ProjectDetailDTO>(project);
            return Ok(mappedProject);
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{projectId}/members")]
    [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
    public async Task<ActionResult<List<MemberWithRoleInProjectResponseDTO>>> GetProjectWithMembers([FromRoute] string projectId)
    {
        try
        {
            var project = await _projectService.GetProjectAndMemberById(projectId);
            var response = _mapper.Map<List<MemberWithRoleInProjectResponseDTO>>(project.UserProjects);
            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, MessageConstant.InternalServerError);
        }
    }
    
    [HttpGet("{projectId}/general-information")]
    [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
    public async Task<ActionResult<ProjectInformationWithMembersResponseDTO>> GetProjectInformationWithMembers([FromRoute] string projectId)
    {
        try
        {
            var project = await _projectService.GetProjectInformationWithMemberById(projectId);
            return Ok(project);
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, MessageConstant.InternalServerError);
        }
    }

    [HttpGet("{projectId}/parties")]
    [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
    public async Task<ActionResult<UserInContractResponseDTO>> GetListUsersRelevantToContractInAProject([FromRoute] string projectId)
    {
        try
        {
            var userList = await _projectService.GetListUserRelevantToContractsInAProject(projectId);
            var responseUserList = _mapper.Map<List<UserInContractResponseDTO>>(userList);
            return Ok(responseUserList);
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, MessageConstant.InternalServerError);
        }
    }

    [HttpGet("/api/startups")]
    [Authorize(Roles = RoleConstants.INVESTOR)]
    public async Task<ActionResult<PaginationDTO<ExploreProjectDTO>>> ExploreProjects([FromQuery] ProjectFilterDTO projectFilterDTO, [FromQuery] int page, int size)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var projectList = await _projectService.GetProjectsForInvestor(userId, projectFilterDTO, size, page);
            return Ok(projectList);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }

    }
    [HttpGet("{projectId}/current-role")]
    [Authorize]
    public async Task<ActionResult<UserRoleInATeamResponseDTO>> GetRoleInTeamInChosenProjectOfACurrentUser([FromRoute] string projectId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var roleInTeam = _mapper.Map<UserRoleInATeamResponseDTO>(userInProject);
            return Ok(roleInTeam);
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedProjectRoleException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPost("{projectId}/payment-gateway")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<IActionResult> RegisterPaymentGatewayForProject([FromRoute] string projectId, [FromBody] PayOsPaymentGatewayRegisterDTO payOsPaymentGatewayRegisterDTO)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            await _projectService.AddPaymentGatewayInfo(userId, projectId, payOsPaymentGatewayRegisterDTO);
            return Ok("Đăng ký cổng thanh toán thành công");
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedProjectRoleException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet("{projectId}/payment-gateway")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<PayOsPaymentGatewayResponseDTO>> GetPaymentGatewayInfo([FromRoute] string projectId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var paymentInfo = await _projectService.GetPaymentGatewayInfoByProjectId(userId, projectId);
            return Ok(paymentInfo);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedProjectRoleException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{projectId}/dashboard")]
    [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
    public async Task<ActionResult<ProjectDashboardDTO>> GetProjectDashboard([FromRoute] string projectId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var dashboard = await _projectService.GetProjectDashboard(userId, projectId);
            return Ok(dashboard);
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedProjectRoleException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPut("{projectId}/close")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<IActionResult> ClosingAProject([FromRoute] string projectId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            await _projectService.CloseAProject(userId, projectId);
            return Ok("Đã đóng dự án thành công");
        }
        catch (UnauthorizedProjectRoleException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (UpdateException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{projectId}/edit-post")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<IActionResult> EditProjectDetailPost([FromRoute] string projectId, ProjectDetailPostDTO projectDetailPostDTO)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            await _projectService.UpdateProjectDetail(userId,projectId,projectDetailPostDTO);
            return Ok("Cập nhật thành công");
        }
        catch (UnauthorizedProjectRoleException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (UpdateException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{projectId}/check-closable")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<ClosingProjectInformationDTO>> GetClosableInformationForProject([FromRoute] string projectId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var response = await _projectService.GetProjectClosingInformation(userId, projectId);
            return Ok(response);
        }
        catch (UnauthorizedProjectRoleException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet("{projectId}/check-leaveable")]
    [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
    public async Task<ActionResult<LeavingProjectInfomationDTO>> GetLeavingInformationForProject([FromRoute] string projectId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var response = await _projectService.GetProjectLeavingInformation(userId, projectId);
            return Ok(response);
        }
        catch (UnauthorizedProjectRoleException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}