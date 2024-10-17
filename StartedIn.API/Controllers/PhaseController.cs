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
    public class PhaseController : ControllerBase
    {
        private readonly IPhaseService _phaseService;
        private readonly ILogger<PhaseController> _logger;
        private readonly IMapper _mapper;

        public PhaseController(IPhaseService phaseService, ILogger<PhaseController> logger, IMapper mapper)
        {
            _phaseService = phaseService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("phase/{phaseId}")]
        public async Task<ActionResult<PhaseDetailResponseDTO>> GetPhaseDetailById(string phaseId)
        {
            try
            {
                var phase = await _phaseService.GetPhaseDetailById(phaseId);
                var mappedPhase = _mapper.Map<PhaseDetailResponseDTO>(phase);
                return Ok(mappedPhase);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("phases/phase")]
        public async Task<ActionResult<PhaseDetailResponseDTO>> CreateNewPhase(PhaseCreateDTO phaseCreateDto)
        {
            try
            {
                var phase = await _phaseService.CreateNewPhase(phaseCreateDto);
                var responsePhase = _mapper.Map<PhaseDetailResponseDTO>(phase);
                return CreatedAtAction(nameof(GetPhaseDetailById), new { phaseId = responsePhase.Id }, responsePhase);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating new phase.");
                return StatusCode(500, "Lỗi server");
            }
        }

    }
}
