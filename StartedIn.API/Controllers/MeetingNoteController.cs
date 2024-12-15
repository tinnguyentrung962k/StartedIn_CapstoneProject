using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Appointment;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;

namespace StartedIn.API.Controllers;

[ApiController]
[Route("api/projects/{projectId}")]
public class MeetingNoteController : ControllerBase
{
    private readonly IMeetingNoteService _meetingNoteService;
    private readonly IMapper _mapper;

    public MeetingNoteController(IMeetingNoteService meetingNoteService, IMapper mapper)
    {
        _meetingNoteService = meetingNoteService;
        _mapper = mapper;
    }
    
    [HttpPost("appointments/{appointmentId}/meeting-note")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<AppointmentResponseDTO>> UploadMeetingNote([FromRoute] string projectId, string appointmentId, [FromForm] UploadMeetingNoteDTO uploadMeetingNoteDto)
    {
        try 
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var meetingNote = await _meetingNoteService.UploadMeetingNote(userId, projectId, appointmentId, uploadMeetingNoteDto);
            var response = _mapper.Map<MeetingNoteResponseDTO>(meetingNote);
            return CreatedAtAction(nameof(GetMeetingNoteById), new { projectId, appointmentId, meetingNoteId = response.Id }, response);
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedProjectRoleException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpGet("appointments/{appointmentId}/meeting-note/{meetingNoteId}")]
    [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
    public async Task<ActionResult<AppointmentResponseDTO>> GetMeetingNoteById([FromRoute] string projectId, [FromRoute] string appointmentId, [FromRoute] string meetingNoteId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var meeting = await _meetingNoteService.GetMeetingNoteById(projectId, appointmentId, meetingNoteId);
            var response = _mapper.Map<MeetingNoteResponseDTO>(meeting);
            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpGet("appointments/{appointmentId}/meeting-note")]
    [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
    public async Task<ActionResult<AppointmentResponseDTO>> GetMeetingNoteByAppointmentId([FromRoute] string projectId, [FromRoute] string appointmentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var meeting = await _meetingNoteService.GetMeetingNoteByAppointmentId(projectId, appointmentId);
            var response = _mapper.Map<List<MeetingNoteResponseDTO>>(meeting);
            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}