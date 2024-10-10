using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;

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

        [HttpPost("milestone/create")]
        public async Task<ActionResult<MilestoneResponseDTO>> CreateNewMajorTask(MilestoneCreateDTO milestoneCreateDto)
        {
            try
            {
                string id = await _milestoneService.CreateNewMilestone(milestoneCreateDto);
                return StatusCode(201, new { message = "Tạo cột mốc thành công", id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating new major task.");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpPut("milestone/move")]
        public async Task<ActionResult<MilestoneResponseDTO>> MoveMilestone(UpdateMilestonePositionDTO updateMilestonePositionDTO)
        {
            try
            {
                var responseMilestone = _mapper.Map<MilestoneResponseDTO>(await _milestoneService.MoveMilestone(updateMilestonePositionDTO.Id, updateMilestonePositionDTO.PhaseId, updateMilestonePositionDTO.Position, updateMilestonePositionDTO.NeedsReposition));
                return Ok(responseMilestone);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("Di chuyển cột mốc thất bại");
            }
        }

        [HttpGet("milestone/{milestoneId}")]
        public async Task<ActionResult<MilestoneAndTaskboardResponseDTO>> GetMilestoneById([FromRoute] string milestoneId)
        {
            try
            {
                var responseMilestone = _mapper.Map<MilestoneAndTaskboardResponseDTO>(await _milestoneService.GetMilestoneById(milestoneId));
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

        [HttpPut("milestone/edit/{id}")]
        public async Task<ActionResult<MilestoneResponseDTO>> EditInfoMilestone(string id, [FromBody] MilestoneInfoUpdateDTO milestoneInfoUpdateDTO)
        {
            try
            {
                var responseMilestone = _mapper.Map<MilestoneResponseDTO>(await _milestoneService.UpdateMilestoneInfo(id, milestoneInfoUpdateDTO));
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
