using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Appointment;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [Route("api/projects/{projectId}")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IMapper _mapper;
        public AppointmentController(IAppointmentService appointmentService, IMapper mapper)
        {
            _appointmentService = appointmentService;
            _mapper = mapper;
        }

        [HttpGet("appointments/{year:int}")]
        [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<List<AppointmentInCalendarResponseDTO>>> GetMeetingsInAYearOfAProject([FromRoute] string projectId, [FromRoute] int year)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var meetings = await _appointmentService.GetAppointmentsInProject(userId, projectId, year);
                var response = _mapper.Map<List<AppointmentInCalendarResponseDTO>>(meetings);
                return Ok(response);
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
        [HttpGet("appointments/{appointmentId}")]
        [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<AppointmentResponseDTO>> GetMeetingsById([FromRoute] string projectId, [FromRoute] string appointmentId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var meeting = await _appointmentService.GetAppointmentsById(userId, projectId, appointmentId);
                var response = _mapper.Map<AppointmentResponseDTO>(meeting);
                return Ok(response);
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

        [HttpPost("appointments")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<AppointmentResponseDTO>> CreateAnAppointment([FromRoute] string projectId, [FromBody] AppointmentCreateDTO appointmentCreateDTO)
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var meeting = await _appointmentService.CreateAnAppointment(userId, projectId, appointmentCreateDTO);
                var response = _mapper.Map<AppointmentResponseDTO>(meeting);
                return CreatedAtAction(nameof(GetMeetingsById), new { projectId, appointmentId = response.Id }, response);
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
    }
}
