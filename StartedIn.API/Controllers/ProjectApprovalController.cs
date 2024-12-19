using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.ProjectApproval;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectApproval;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;

namespace StartedIn.API.Controllers;

[Route("api/projects")]
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
    
    [HttpGet("{projectId}/approval")]
    [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
    public async Task<ActionResult<ProjectApprovalResponseDTO>> GetProjectApprovalById(string projectId)
    {
        try
        {
            var projectApproval = await _projectApprovalService.GetProjectApprovalRequestByProjectId(projectId);
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

    [HttpPost("{projectId}/request-approval")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<ProjectApprovalResponseDTO>> CreateProjectApprovalRequest(string userId,
        string projectId, CreateProjectApprovalDTO createProjectApprovalDto)
    {
        try
        {
            var projectApproval =
                await _projectApprovalService.CreateProjectApprovalRequest(userId, projectId, createProjectApprovalDto);
            var response = _mapper.Map<ProjectApprovalResponseDTO>(projectApproval);
            return CreatedAtAction(nameof(GetProjectApprovalById), new { projectId }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    

    [HttpPut("{projectId}/approve-project-request")]
    [Authorize(Roles = RoleConstants.ADMIN)]
    public async Task<ActionResult> ApproveProjectRequest([FromRoute] string projectId)
    {
        try
        {
            await _projectApprovalService.ApproveProjectRequest(projectId);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPut("{projectId}/reject-project-request")]
    [Authorize(Roles = RoleConstants.ADMIN)]
    public async Task<ActionResult> RejectProjectRequest([FromRoute] string projectId, [FromBody] string rejectReason)
    {
        try
        {
            await _projectApprovalService.RejectProjectRequest(projectId, rejectReason);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}