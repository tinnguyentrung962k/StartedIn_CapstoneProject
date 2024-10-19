using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class TaskboardController : ControllerBase
    {
        private readonly ITaskboardService _taskboardService;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskboardController> _logger;
        
        public TaskboardController(ITaskboardService taskboardService,
            ILogger<TaskboardController> logger, IMapper mapper)
        {
            _taskboardService = taskboardService;
            _logger = logger;
            _mapper = mapper;
        }
        
        [HttpPost("taskboards")]
        [Authorize]
        public async Task<ActionResult<TaskboardResponseDTO>> CreateNewTaskboard(TaskboardCreateDTO taskboardCreateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var taskboard = await _taskboardService.CreateNewTaskboard(taskboardCreateDto, userId);
                var responseTaskboard = _mapper.Map<TaskboardResponseDTO>(taskboard);
                return CreatedAtAction(nameof(GetTaskboardById), new { taskboardId = responseTaskboard.Id }, responseTaskboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating new task.");
                return StatusCode(500, "Lỗi server");
            }
        }

        
        [HttpPut("taskboards/move")]
        [Authorize]
        public async Task<ActionResult<TaskboardResponseDTO>> MoveTaskBoard(UpdateTaskboardPositionDTO updateTaskboardPositionDTO)
        {
            try
            {
                var responseTaskBoard = _mapper.Map<TaskboardResponseDTO>(await _taskboardService.MoveTaskBoard(updateTaskboardPositionDTO.Id, updateTaskboardPositionDTO.Position, updateTaskboardPositionDTO.NeedsReposition));
                return Ok(responseTaskBoard);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("Di chuyển bảng làm việc thất bại");
            }
        }
        [HttpGet("taskboards/{taskboardId}")]
        [Authorize]
        public async Task<ActionResult<TaskboardResponseDTO>> GetTaskboardById([FromRoute] string taskboardId)
        {
            try
            {
                var responseTaskBoard = _mapper.Map<TaskboardResponseDTO>(await _taskboardService.GetTaskboardById(taskboardId));
                return Ok(responseTaskBoard);
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

        [HttpPut("taskboards/{taskboardId}")]
        [Authorize]
        public async Task<ActionResult<TaskboardResponseDTO>> UpdateTaskboard([FromRoute] string taskboardId, [FromBody] TaskboardInfoUpdateDTO taskboardInfoUpdateDTO)
        {
            try
            {
                var responseTaskBoard = _mapper.Map<TaskboardResponseDTO>(await _taskboardService.UpdateTaskboard(taskboardId, taskboardInfoUpdateDTO));
                return Ok(responseTaskBoard);
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
    }
}
