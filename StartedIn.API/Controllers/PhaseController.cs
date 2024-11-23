using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Phase;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Phase;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;

namespace StartedIn.API.Controllers;
[ApiController]
[Route("api/projects/{projectId}")]
public class PhaseController : ControllerBase
{
    private readonly IPhaseService _phaseService;
    private readonly IMapper _mapper;

    public PhaseController(IPhaseService phaseService, IMapper mapper)
    {
        _phaseService = phaseService;
        _mapper = mapper;
    }

    [HttpPost("{projectCharterId}/phase")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<PhaseResponseDTO>> CreateNewPhase([FromRoute] string projectId, string projectCharterId, CreatePhaseDTO createPhaseDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var phase = await _phaseService.CreateNewPhase(userId, projectId, projectCharterId, createPhaseDto);
            var response = _mapper.Map<PhaseResponseDTO>(phase);
            return CreatedAtAction(nameof(GetPhaseByPhaseId), new { projectId, projectCharterId, phaseId = response.Id }, response);
        }
        catch (UnauthorizedProjectRoleException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidInputException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, MessageConstant.InternalServerError);
        }
    }

    [HttpGet("{projectCharterId}/{phaseId}")]
    public async Task<ActionResult<PhaseResponseDTO>> GetPhaseByPhaseId([FromRoute]string projectCharterId, string phaseId)
    {
        try
        {
            var phase = await _phaseService.GetPhaseByPhaseId(projectCharterId, phaseId);
            var response = _mapper.Map<PhaseResponseDTO>(phase);
            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [HttpGet("phases")]
    public async Task<ActionResult<List<PhaseResponseDTO>>> GetPhasesByProjectId([FromRoute] string projectId)
    {
        var phases = await _phaseService.GetPhasesByProjectId(projectId);
        var response = _mapper.Map<List<PhaseResponseDTO>>(phases);
        return Ok(response);
    }
}