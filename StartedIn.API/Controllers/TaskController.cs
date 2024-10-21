using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
    public class TaskController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<TaskController> _logger;
        private readonly ITaskService _taskService;

        public TaskController(IMapper mapper, ILogger<TaskController> logger, ITaskService taskService)
        {
            _taskService = taskService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("tasks")]
        [Authorize]
        public async Task<ActionResult<TaskResponseDTO>> CreateNewTask(TaskCreateDTO taskCreateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var task = await _taskService.CreateTask(taskCreateDto, userId);
                var response = _mapper.Map<TaskResponseDTO>(task);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating new task.");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpPut("tasks/{taskId}")]
        [Authorize]
        public async Task<ActionResult<TaskResponseDTO>> EditInfoMinorTask([FromRoute]string taskId, [FromBody] UpdateTaskInfoDTO updateTaskInfoDTO)
        {
            try
            {
                var responseTask = _mapper.Map<TaskResponseDTO>(await _taskService.UpdateTaskInfo(taskId, updateTaskInfoDTO));
                return Ok(responseTask);
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
