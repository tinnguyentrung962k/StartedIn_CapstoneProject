using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Recruitment;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;

namespace StartedIn.API.Controllers;

[ApiController]
[Route("api")]
public class RecruitmentController : ControllerBase
{
    private readonly IRecruitmentService _recruitmentService;
    private readonly IMapper _mapper;
    private readonly IRecruitmentImageService _recruitmentImageService;

    public RecruitmentController(IRecruitmentService recruitmentService, IMapper mapper, IRecruitmentImageService recruitmentImageService)
    {
        _recruitmentService = recruitmentService;
        _mapper = mapper;
        _recruitmentImageService = recruitmentImageService;
    }

    // Create recruitment post for users in the project
    [HttpPost("projects/{projectId}/recruitments")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<RecruitmentResponseDTO>> CreateRecruitmentPost([FromRoute] string projectId,
        [FromForm] CreateRecruitmentDTO createRecruitmentDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var recruitment = await _recruitmentService.CreateRecruitment(projectId, userId, createRecruitmentDto);
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

    // Get created recruitment post info for users in the project
    [HttpGet("projects/{projectId}/recruitments")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<RecruitmentInProjectDTO>> GetRecruitmentPostInProject([FromRoute] string projectId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var recruitment = await _recruitmentService.GetRecruitmentPostInProject(userId, projectId);
            var response = _mapper.Map<RecruitmentInProjectDTO>(recruitment);
            return Ok(response);
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


    // Update recruitment post for users in the project
    [HttpPut("projects/{projectId}/recruitments")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<RecruitmentResponseDTO>> UpdateRecruitment([FromRoute] string projectId,
        string recruitmentId, [FromBody] UpdateRecruitmentDTO updateRecruitmentDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var recruitment = await _recruitmentService.UpdateRecruitment(userId, projectId, recruitmentId, updateRecruitmentDto);
            var response = _mapper.Map<RecruitmentResponseDTO>(recruitment);
            return Ok(response);
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

    // Get recruitment list for guests / outside users
    [HttpGet("recruitments")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<PaginationDTO<RecruitmentListDTO>>> GetRecruitmentList([FromQuery] int page, [FromQuery] int size)
    {
        try
        {
            var recruitmentList = await _recruitmentService.GetRecruitmentListWithLeader(page, size);
            return Ok(recruitmentList);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    // Get recruitment details in the recruitment list of guests / outside users
    [HttpGet("recruitments/{recruitmentId}")]
    [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR)]
    public async Task<ActionResult<RecruitmentResponseDTO>> GetRecruitmentPostById([FromRoute]
       string recruitmentId)
    {
        var recruitment = await _recruitmentService.GetRecruitmentPostById(recruitmentId);
        var response = _mapper.Map<RecruitmentResponseDTO>(recruitment);
        return Ok(response);
    }

    [HttpPost("projects/{projectId}/recruitments/images/add")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<List<RecruitmentImgResponseDTO>>> AddImageToRecruitmentPost([FromRoute] string projectId,
        [FromForm] List<IFormFile> recruitFiles)
    {
        try
        {
            //TODO: Check if user is in project
            var listImages = await _recruitmentImageService.AddImageToRecruitmentPost(projectId, recruitFiles);
            var response = _mapper.Map<List<RecruitmentImgResponseDTO>>(listImages);
            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("projects/{projectId}/recruitments/images/{recruitmentImgId}/remove")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<IActionResult> RemoveImageFromRecruitmentPost([FromRoute] string projectId,
        string recruitmentImgId)
    {
        try
        {
            //TODO: Check if user is in project
            await _recruitmentImageService.RemoveImageFromRecruitmentPost(projectId, recruitmentImgId);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}