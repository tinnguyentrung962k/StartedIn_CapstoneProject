using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using StartedIn.API.Attributes;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Milestone;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone;
using StartedIn.CrossCutting.DTOs.ResponseDTO;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/milestones")]
    public class MilestoneController : ControllerBase
    {
        private readonly IMilestoneService _milestoneService;
        private readonly IMapper _mapper;
        private readonly ILogger<MilestoneController> _logger;

        public MilestoneController(IMilestoneService milestoneService, IMapper mapper, ILogger<MilestoneController> logger)
        {
            _milestoneService = milestoneService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<MilestoneResponseDTO>> CreateNewMileStone([FromRoute] string projectId, [FromBody] MilestoneCreateDTO milestoneCreateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var milestone = await _milestoneService.CreateNewMilestone(userId, projectId, milestoneCreateDto);
                var responseMilestone = _mapper.Map<MilestoneResponseDTO>(milestone);
                return CreatedAtAction(nameof(GetMilestoneById), new { projectId, milestoneId = responseMilestone.Id }, responseMilestone);

            }
            catch (UnauthorizedProjectRoleException ex)
            {
                _logger.LogError(ex, "Unauthorized Role");
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating new task.");
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpGet("{milestoneId}")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<MilestoneDetailsResponseDTO>> GetMilestoneById([FromRoute] string projectId, [FromRoute] string milestoneId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var responseMilestone = _mapper.Map<MilestoneDetailsResponseDTO>(await _milestoneService.GetMilestoneById(userId, projectId, milestoneId));
                return Ok(responseMilestone);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnmatchedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                _logger.LogError(ex, "Unauthorized Role");
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpGet]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<PaginationDTO<MilestoneResponseDTO>>> GetMilestonesForProject(
            [FromRoute] string projectId,
            [FromQuery] MilestoneFilterDTO milestoneFilterDTO,
            [FromQuery] int page,
            [FromQuery] int size)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var responseMilestone = await _milestoneService.FilterMilestone(userId, projectId, milestoneFilterDTO, page, size);
                return Ok(responseMilestone);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpPut("{milestoneId}")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<MilestoneResponseDTO>> EditInfoMilestone([FromRoute] string milestoneId, [FromRoute] string projectId, [FromBody] MilestoneInfoUpdateDTO milestoneInfoUpdateDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var responseMilestone = _mapper.Map<MilestoneResponseDTO>(await _milestoneService.UpdateMilestoneInfo(userId, projectId, milestoneId, milestoneInfoUpdateDTO));
                return Ok(responseMilestone);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                _logger.LogError(ex, "Unauthorized Role");
                return StatusCode(403, ex.Message);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnmatchedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(MessageConstant.UpdateFailed);
            }
        }

        [HttpDelete("{milestoneId}")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult> DeleteMilestone([FromRoute] string projectId, [FromRoute] string milestoneId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _milestoneService.DeleteMilestone(userId, projectId, milestoneId);
                return Ok();
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                _logger.LogError(ex, "Unauthorized Role");
                return StatusCode(403, ex.Message);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnmatchedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(MessageConstant.DeleteFailed);
            }
        }
    }
}
