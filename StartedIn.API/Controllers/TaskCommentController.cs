using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.TaskComment;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/tasks/{taskId}/comment")]
    public class TaskCommentController : ControllerBase
    {
        private readonly ITaskCommentService _taskCommentService;
        private readonly IMapper _mapper;

        public TaskCommentController(ITaskCommentService taskCommentService, IMapper mapper)
        {
            _taskCommentService = taskCommentService;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<TaskCommentCreateDTO>> AddComment([FromRoute] string projectId, [FromRoute] string taskId, [FromBody] TaskCommentCreateDTO taskCommentCreateDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var taskComment = await _taskCommentService.AddComment(projectId, taskId, userId, taskCommentCreateDTO);
                var responseTaskComment = _mapper.Map<TaskCommentCreateDTO>(taskComment);
                return Ok(responseTaskComment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
