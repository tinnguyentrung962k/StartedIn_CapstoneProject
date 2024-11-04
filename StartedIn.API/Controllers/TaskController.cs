using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
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
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
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
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var responseTask = _mapper.Map<TaskResponseDTO>(await _taskService.UpdateTaskInfo(userId,taskId, updateTaskInfoDTO));
                return Ok(responseTask);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("Cập nhật thất bại");
            }
        }
        [HttpGet("tasks/{taskId}")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<TaskResponseDTO>> GetTaskDetail([FromRoute] string taskId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var responseTask = _mapper.Map<TaskResponseDTO>(await _taskService.GetATaskDetail(userId,taskId));
                return Ok(responseTask);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("Truy xuất thất bại");
            }
        }

    }
}
