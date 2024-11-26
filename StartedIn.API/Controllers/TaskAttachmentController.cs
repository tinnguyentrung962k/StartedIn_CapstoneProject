using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.TaskAttachment;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TaskAttachment;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;

namespace StartedIn.API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/tasks/{taskId}/task-attachment")]
public class TaskAttachmentController : ControllerBase
{
    private readonly ITaskAttachmentService _taskAttachmentService;
    private readonly IMapper _mapper;

    public TaskAttachmentController(ITaskAttachmentService taskAttachmentService, IMapper mapper)
    {
        _taskAttachmentService = taskAttachmentService;
        _mapper = mapper;
    }

    [HttpPost]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<TaskAttachmentResponseDTO>> CreateTaskAttachment(TaskAttachmentCreateDTO taskAttachmentCreateDto)
    {
        try
        {
            var taskAttachment = await _taskAttachmentService.AddTaskAttachment(taskAttachmentCreateDto);
            var response = _mapper.Map<TaskAttachmentResponseDTO>(taskAttachment);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{taskAttachmentId}")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult> DeleteTaskAttachment([FromRoute] string taskAttachmentId)
    { 
        try
        {
            await _taskAttachmentService.DeleteTaskAttachment(taskAttachmentId);
            return Ok();
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