using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
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
        
        [HttpPost("taskboard/create")]
        [Authorize]
        public async Task<ActionResult<TaskboardResponseDTO>> CreateNewTaskboard(TaskboardCreateDTO taskboardCreateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var taskboard = await _taskboardService.CreateNewTaskboard(taskboardCreateDto, userId);
                var response = _mapper.Map<TaskboardResponseDTO>(taskboard);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating new task.");
                return StatusCode(500, "Lỗi server");
            }
        }
    }
}
