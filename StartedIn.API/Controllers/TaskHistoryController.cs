using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TaskHistory;
using StartedIn.Service.Services.Interface;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/tasks")]
    public class TaskHistoryController : ControllerBase
    {
        private readonly ITaskHistoryService _taskHistoryService;
        private readonly IMapper _mapper;

        public TaskHistoryController(ITaskHistoryService taskHistoryService, IMapper mapper)
        {
            _taskHistoryService = taskHistoryService;
            _mapper = mapper;
        }

        // Get Task History of a single Task
        [HttpGet("{taskId}/history")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<IEnumerable<TaskHistoryResponseDTO>>> GetTaskHistoryOfTask([FromRoute] string projectId, string taskId, [FromQuery] string userId)
        {
            try
            {
                var taskHistory = await _taskHistoryService.GetTaskHistoryOfTask(projectId, taskId, userId);
                var response = _mapper.Map<IEnumerable<TaskHistoryResponseDTO>>(taskHistory);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Get Task History of the whole project
        [HttpGet("history")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<PaginationDTO<TaskHistoryResponseDTO>>> GetTaskHistoryById([FromRoute] string projectId, [FromQuery] int page, [FromQuery] int size)
        {
            try
            {
                var taskHistory = await _taskHistoryService.GetTaskHistoriesOfProject(projectId, page, size);
                return Ok(taskHistory);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
