using System.Security.Claims;
using AutoMapper;
using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.ProjectApproval;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectApproval;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;

namespace StartedIn.API.Controllers;

[Route("api")]
[ApiController]
public class ProjectApprovalController : ControllerBase
{
    private readonly IProjectApprovalService _projectApprovalService;
    private readonly IMapper _mapper;

    public ProjectApprovalController(IProjectApprovalService projectApprovalService, IMapper mapper)
    {
        _projectApprovalService = projectApprovalService;
        _mapper = mapper;
    }

    [HttpGet("approvals")]
    [Authorize(Roles = RoleConstants.ADMIN)]
    public async Task<ActionResult<PaginationDTO<ProjectApprovalResponseDTO>>> GetAllProjectApprovals([FromQuery] ProjectApprovalFilterDTO projectApprovalFilterDto, 
        [FromQuery] int page, [FromQuery] int size)
    {
        try
        { 
            var projectApprovals = await _projectApprovalService.GetAllProjectApprovals(projectApprovalFilterDto, page, size);
            return Ok(projectApprovals);
        }
        catch (Exception ex)
        {
            return StatusCode(500, MessageConstant.InternalServerError + ex.Message);
        }
    }
    
    [HttpGet("projects/{projectId}/request-register/{approvalId}")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<ProjectApprovalResponseDTO>> GetProjectApprovalByApprovalId([FromRoute] string projectId, [FromRoute] string approvalId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var projectApproval = await _projectApprovalService.GetProjectApprovalRequestByApprovalId(userId, projectId, approvalId);
            var response = _mapper.Map<ProjectApprovalResponseDTO>(projectApproval);
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
    
    [HttpGet("projects/{projectId}/request-register")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<List<ProjectApprovalResponseDTO>>> GetProjectApprovalsForProject([FromRoute] string projectId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var projectApproval = await _projectApprovalService.GetProjectApprovalRequestByProjectId(userId, projectId);
            var response = _mapper.Map<List<ProjectApprovalResponseDTO>>(projectApproval);
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
    
    [HttpPost("projects/{projectId}/request-approval")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<ProjectApprovalResponseDTO>> CreateProjectApprovalRequest([FromRoute]string projectId, [FromForm] CreateProjectApprovalDTO createProjectApprovalDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var projectApproval =
                await _projectApprovalService.CreateProjectApprovalRequest(userId, projectId, createProjectApprovalDto);
            var response = _mapper.Map<ProjectApprovalResponseDTO>(projectApproval);
            return CreatedAtAction(nameof(GetProjectApprovalByApprovalId),
                new { projectId, approvalId = projectApproval.Id }, response);
        }
        catch (ExistedRecordException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    

    [HttpPut("approvals/{approvalId}/approve-project-request")]
    [Authorize(Roles = RoleConstants.ADMIN)]
    public async Task<ActionResult> ApproveProjectRequest([FromRoute] string approvalId)
    {
        try
        {
            await _projectApprovalService.ApproveProjectRequest(approvalId);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPut("approvals/{approvalId}/reject-project-request")]
    [Authorize(Roles = RoleConstants.ADMIN)]
    public async Task<ActionResult> RejectProjectRequest([FromRoute] string approvalId, [FromBody] string rejectReason)
    {
        try
        {
            await _projectApprovalService.RejectProjectRequest(approvalId, rejectReason);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}