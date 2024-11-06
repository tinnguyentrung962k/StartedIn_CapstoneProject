using AutoMapper;
using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
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

    public ProjectController(IProjectService projectService, IMapper mapper, ILogger<ProjectController> logger)
    {
        _projectService = projectService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ProjectListDTO>> GetListOfProjectsWithRole()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        try
        {
            var ownedProjects = _mapper.Map<List<ProjectResponseDTO>>(await _projectService.GetListOwnProjects(userId));
            var participatedProjects = _mapper.Map<List<ProjectResponseDTO>>(await _projectService.GetListParticipatedProjects(userId));
            var response = new ProjectListDTO
            {
                listOwnProject = ownedProjects,
                listParticipatedProject = participatedProjects
            };
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProjectResponseDTO>> CreateANewProject([FromForm] ProjectCreateDTO projectCreatedto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var newProject = _mapper.Map<Project>(projectCreatedto);
            await _projectService.CreateNewProject(userId, newProject, projectCreatedto.LogoFile);
            var responseNewProject = _mapper.Map<ProjectResponseDTO>(newProject);
            return CreatedAtAction(nameof(GetProjectById), new { projectId = responseNewProject.Id }, responseNewProject);
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
    [Authorize]
    public async Task<ActionResult<ProjectResponseDTO>> GetProjectById(string projectId)
    {
        try
        {
            var project = await _projectService.GetProjectById(projectId);
            var mappedProject = _mapper.Map<ProjectResponseDTO>(project);
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

    [HttpPost("{projectId}/invite")]
    [Authorize]
    public async Task<IActionResult> SendInvitationToTeam([FromBody] List<ProjectInviteEmailAndRoleDTO> inviteUsers, [FromRoute] string projectId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            await _projectService.SendJoinProjectInvitation(userId, inviteUsers, projectId);
            return Ok("Gửi lời mời gia nhập thành công");
        }
        catch (InviteException ex)
        {
            return BadRequest(ex.Message);
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

    [HttpPost("{projectId}/join")]
    [Authorize]
    public async Task<IActionResult> AddUserToProject([FromRoute] string projectId, RoleInTeam roleInTeam)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            await _projectService.AddUserToProject(projectId, userId, roleInTeam);
            return Ok("Bạn đã được tham gia dự án!");
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InviteException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{projectId}/members")]
    [Authorize]
    public async Task<ActionResult<ProjectWithMembersResponseDTO>> GetProjectWithMembers([FromRoute] string projectId)
    {
        try
        {
            var project = await _projectService.GetProjectAndMemberById(projectId);
            var response = _mapper.Map<ProjectWithMembersResponseDTO>(project);
            return Ok(response);
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


    [HttpGet("projects/user-projects")]
    [Authorize]
    public async Task<ActionResult<ProjectListDTO>> GetListOfProjectsWithRole()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        try
        {
            var ownedProjects = await _projectService.GetListOwnProjects(userId);
            var participatedProjects = await _projectService.GetListParticipatedProjects(userId);
            var response = new ProjectListDTO
            {
                listOwnProject = ownedProjects,
                listParticipatedProject = participatedProjects
            };
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpGet("{projectId}/parties")]
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
    [Authorize]
    public async Task<ActionResult<SearchResponseDTO<ExploreProjectDTO>>> ExploreProjects([FromQuery] int pageIndex, int pageSize)
    {
        try
        {
            pageIndex = pageIndex < 1 ? 1 : pageIndex;
            pageSize = pageSize < 1 ? 10 : pageSize;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var result = await _projectService.GetProjectsForInvestor(userId, pageIndex, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
            
    }
}