using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using StartedIn.CrossCutting.Constants;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api")]
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

        [HttpPost("milestones")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<MilestoneResponseDTO>> CreateNewMileStone(MilestoneCreateDTO milestoneCreateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var milestone = await _milestoneService.CreateNewMilestone(userId, milestoneCreateDto);
                var responseMilestone = _mapper.Map<MilestoneResponseDTO>(milestone);
                return CreatedAtAction(nameof(GetMilestoneById), new { milestoneId = responseMilestone.Id }, responseMilestone);

            }
            catch (UnauthorizedProjectRoleException ex)
            {
                _logger.LogError(ex, "Unauthorized Role");
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating new major task.");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpGet("milestones/{milestoneId}")]
        [Authorize]
        public async Task<ActionResult<MilestoneAndTaskResponseDTO>> GetMilestoneById([FromRoute] string milestoneId)
        {
            try
            {
                var responseMilestone = _mapper.Map<MilestoneAndTaskResponseDTO>(await _milestoneService.GetMilestoneById(milestoneId));
                return Ok(responseMilestone);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi Server");
            }
        }

        [HttpPut("milestones/{milestoneId}")]
        [Authorize]
        public async Task<ActionResult<MilestoneResponseDTO>> EditInfoMilestone(string milestoneId, [FromBody] MilestoneInfoUpdateDTO milestoneInfoUpdateDTO)
        {
            try
            {
                var responseMilestone = _mapper.Map<MilestoneResponseDTO>(await _milestoneService.UpdateMilestoneInfo(milestoneId, milestoneInfoUpdateDTO));
                return Ok(responseMilestone);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("Cập nhật thất bại");
            }
        }
    }
}
