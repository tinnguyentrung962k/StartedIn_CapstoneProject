using System.Security.Claims;
using AutoMapper;
using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;

namespace StartedIn.API.Controllers;

[ApiController]
[Route("api")]
public class ProjectController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ILogger<ProjectController> _logger;
    private readonly IProjectService _projectService;
    
    [HttpPost("teams")]
    [Authorize]
    public async Task<ActionResult<ProjectResponseDTO>> CreateNewProject(ProjectCreateDTO projectCreateDto) 
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var project = await _projectService.CreateNewProject(userId, projectCreateDto);
            var response = _mapper.Map<ProjectResponseDTO>(project);
            return CreatedAtAction(nameof(GetProjectById), new { id = response.Id }, response);
        }
        catch (ExistedRecordException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest("Tạo team thất bại.");
        }
    }
    
    [HttpGet("projects/{id}")]
    public async Task<ActionResult<ProjectResponseDTO>> GetProjectById(string id)
    {
        try
        {
            var project = await _projectService.GetProjectById(id);
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
    
    [HttpPost("teams/team-invitation/{teamId}")]
    [Authorize]
    public async Task<IActionResult> SendInvitationToTeam([FromBody] List<string> userIds, [FromRoute] string teamId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            await _projectService.SendJoinProjectInvitation(userId, userIds, teamId);
            return Ok("Gửi lời mời gia nhập thành công");
        }
        catch (TeamLimitException ex)
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
}