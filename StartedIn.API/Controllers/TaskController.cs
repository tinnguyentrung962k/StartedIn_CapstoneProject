using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Tasks;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/tasks")]
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

        [HttpPost]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<TaskResponseDTO>> CreateTask(
            [FromBody] TaskCreateDTO taskCreateDto,
            [FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var task = await _taskService.CreateTask(taskCreateDto, userId, projectId);
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
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpGet]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<PaginationDTO<TaskResponseDTO>>> getAllTasks(
            [FromRoute] string projectId,
            [FromQuery] int page = 1,
            [FromQuery] int size = 20)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var taskPagination = await _taskService.GetAllTask(userId, projectId, size, page);

                return taskPagination;
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

        [HttpGet("catalog")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<IEnumerable<TaskResponseDTO>>> getTaskCatalog(
            [FromRoute] string projectId,
            [FromQuery] string? title,
            [FromQuery] TaskEntityStatus? status = null,
            [FromQuery] bool? isLate = false)
        {
            try
            {
                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError + ex.Message);
            }
        }

        [HttpGet("{taskId}")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<TaskDetailDTO>> GetTaskDetail([FromRoute] string projectId, [FromRoute] string taskId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var responseTask = _mapper.Map<TaskDetailDTO>(await _taskService.GetTaskDetail(userId, taskId, projectId));
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
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpPut("{taskId}")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<TaskResponseDTO>> EditTaskInfo(
            [FromRoute] string projectId,
            [FromRoute] string taskId,
            [FromBody] UpdateTaskInfoDTO updateTaskInfoDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var responseTask = _mapper.Map<TaskResponseDTO>(await _taskService.UpdateTaskInfo(userId, taskId, projectId, updateTaskInfoDTO));
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
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpPatch("{taskId}/status")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<TaskResponseDTO>> UpdateTaskStatus(
            [FromRoute] string projectId,
            [FromRoute] string taskId,
            [FromBody] UpdateTaskStatusDTO updateTaskStatusDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var responseTask = _mapper.Map<TaskResponseDTO>(await _taskService.UpdateTaskStatus(userId, taskId, projectId, updateTaskStatusDTO));
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
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        // Separate Assignment Update
        [HttpPatch("{taskId}/assign")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<TaskResponseDTO>> UpdateTaskAssignment(
            [FromRoute] string projectId,
            [FromRoute] string taskId,
            [FromBody] UpdateTaskAssignmentDTO updateTaskAssignmentDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var responseTask = _mapper.Map<TaskResponseDTO>(await _taskService.UpdateTaskAssignment(userId, taskId, projectId, updateTaskAssignmentDTO));
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
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpPatch("{taskId}/unassign")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<TaskResponseDTO>> UnassignTask(
            [FromRoute] string projectId,
            [FromRoute] string taskId,
            [FromBody] UpdateTaskAssignmentDTO updateTaskAssignmentDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var responseTask = _mapper.Map<TaskResponseDTO>(await _taskService.UpdateTaskUnassignment(userId, taskId, projectId, updateTaskAssignmentDTO));
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
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        // Separate Milestone Update
        [HttpPatch("{taskId}/milestone")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<TaskResponseDTO>> UpdateTaskMilestone(
            [FromRoute] string projectId,
            [FromRoute] string taskId,
            [FromBody] UpdateTaskMilestoneDTO updateTaskMilestoneDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var responseTask = _mapper.Map<TaskResponseDTO>(await _taskService.UpdateTaskMilestone(userId, taskId, projectId, updateTaskMilestoneDTO));
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
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpPatch("{taskId}/parent")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<TaskResponseDTO>> UpdateParentTask(
            [FromRoute] string projectId,
            [FromRoute] string taskId,
            [FromBody] UpdateParentTaskDTO updateParentTaskDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var responseTask = _mapper.Map<TaskResponseDTO>(await _taskService.UpdateParentTask(userId, taskId, projectId, updateParentTaskDTO));
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
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }


        [HttpDelete("{taskId}")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult> DeleteTask([FromRoute] string taskId, [FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _taskService.DeleteTask(userId, taskId, projectId);
                return Ok();
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
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }
    }
}
