using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Recruitment;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;

namespace StartedIn.API.Controllers;

[ApiController]
[Route("api/projects/{projectId}")]
public class RecruitmentController : ControllerBase
{
    private readonly IRecruitmentService _recruitmentService;
    private readonly IMapper _mapper;

    public RecruitmentController(IRecruitmentService recruitmentService, IMapper mapper)
    {
        _recruitmentService = recruitmentService;
        _mapper = mapper;
    }

    [HttpPost("recruitment")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<RecruitmentResponseDTO>> CreateRecruitmentPost([FromRoute] string projectId,
        [FromForm] CreateRecruitmentDTO createRecruitmentDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var recruitment = await _recruitmentService.CreateRecruitment(userId, projectId, createRecruitmentDto);
            var response = _mapper.Map<RecruitmentResponseDTO>(recruitment);
            return CreatedAtAction(nameof(GetRecruitmentPostById), new { projectId, recruitmentId = recruitment.Id },
                response);
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedProjectRoleException ex)
        {
            return StatusCode(403, ex.Message);
        }
    }

    [HttpGet("recruitment/{recruitmentId}")]
    [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR)]
    public async Task<ActionResult<RecruitmentResponseDTO>> GetRecruitmentPostById([FromRoute] string projectId,
        string recruitmentId)
    {
        var recruitment = await _recruitmentService.GetRecruitmentPostById(projectId, recruitmentId);
        var response = _mapper.Map<RecruitmentResponseDTO>(recruitment);
        return Ok(response);
    }
}