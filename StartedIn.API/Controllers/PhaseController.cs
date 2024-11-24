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

    [HttpPost("phases")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<PhaseResponseDTO>> CreateNewPhase([FromRoute] string projectId, CreatePhaseDTO createPhaseDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var phase = await _phaseService.CreateNewPhase(userId, projectId, createPhaseDto);
            var response = _mapper.Map<PhaseResponseDTO>(phase);
            return CreatedAtAction(nameof(GetPhaseByPhaseId), new { projectId, phaseId = response.Id }, response);
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

    [HttpGet("phases/{phaseId}")]
    [Authorize]
    public async Task<ActionResult<PhaseResponseDTO>> GetPhaseByPhaseId([FromRoute]string projectId, string phaseId)
    {
        try
        {
            var phase = await _phaseService.GetPhaseByPhaseId(projectId, phaseId);
            var response = _mapper.Map<PhaseResponseDTO>(phase);
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

    [HttpGet("phases")]
    [Authorize]
    public async Task<ActionResult<List<PhaseResponseDTO>>> GetPhasesByProjectId([FromRoute] string projectId)
    {
        try
        {
            var phases = await _phaseService.GetPhasesByProjectId(projectId);
            var response = _mapper.Map<List<PhaseResponseDTO>>(phases);
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
}