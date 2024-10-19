using AutoMapper;
using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers;

[ApiController]
[Route("api")]
public class ProjectController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ILogger<ProjectController> _logger;
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService,IMapper mapper, ILogger<ProjectController> logger)
    {
        _projectService = projectService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpPost("projects")]
    [Authorize]
    public async Task<ActionResult<ProjectResponseDTO>> CreateANewProject(ProjectCreateDTO projectCreatedto) 
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var newProject = _mapper.Map<Project>(projectCreatedto);
            await _projectService.CreateNewProject(userId, newProject);
            var responseNewProject = _mapper.Map<ProjectResponseDTO>(newProject);
            return CreatedAtAction(nameof(GetProjectById), new { projectId = responseNewProject.Id }, responseNewProject);
        }
        catch (ExistedRecordException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest("Tạo dự án thất bại.");
        }
    }
    
    [HttpGet("projects/{projectId}")]
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
    
    [HttpPost("projects/{projectId}/project-invitation")]
    [Authorize]
    public async Task<IActionResult> SendInvitationToTeam([FromBody] List<string> emails, [FromRoute] string projectId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            await _projectService.SendJoinProjectInvitation(userId, emails, projectId);
            return Ok("Gửi lời mời gia nhập thành công");
        }
        catch (InviteException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("projects/{projectId}/invite")]
    [Authorize]
    public async Task<IActionResult> AddUserToProject([FromRoute] string projectId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            await _projectService.AddUserToProject(projectId, userId);
            return Ok("Bạn đã được tham gia dự án!");
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InviteException ex)
        {
            return BadRequest("Người dùng đã tồn tại trong dự án");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpGet("projects/{projectId}/project-members")]
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
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}