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
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace StartedIn.API.Controllers
{
    [Route("api/projects/{projectId}")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IMapper _mapper;
        private readonly IGoogleMeetService _googleMeetService;
        public AppointmentController(IAppointmentService appointmentService, IMapper mapper, IGoogleMeetService googleMeetService)
        {
            _appointmentService = appointmentService;
            _mapper = mapper;
            _googleMeetService = googleMeetService;
        }

        [HttpGet("appointments/{year:int}")]
        [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<List<AppointmentInCalendarResponseDTO>>> GetMeetingsInAYearOfAProject([FromRoute] string projectId, [FromRoute] int year)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var meetings = await _appointmentService.GetAppointmentsInProject(userId, projectId, year);
                var response = _mapper.Map<List<AppointmentResponseDTO>>(meetings);
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
        
        [HttpGet("appointments")]
        [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<List<AppointmentInCalendarResponseDTO>>> GetMeetingsByProjectId([FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var meetings = await _appointmentService.GetAppointmentsByProjectId(userId, projectId);
                var response = _mapper.Map<List<AppointmentResponseDTO>>(meetings);
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
        
        [HttpPost]
        [Route("api/meet/create")]
        public async Task<ActionResult> CreateMeetSession([FromBody] MeetRequestDTO request)
        {
            var service = _googleMeetService.GetCalendarService();

            // Create the event
            Event newEvent = new Event()
            {
                Summary = request.Summary,
                Start = new EventDateTime()
                {
                    DateTime = request.StartTime,
                    TimeZone = "Asia/Bangkok"
                },
                End = new EventDateTime()
                {
                    DateTime = request.EndTime,
                    TimeZone = "Asia/Bangkok"
                },
                ConferenceData = new ConferenceData()
                {
                    CreateRequest = new CreateConferenceRequest()
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        ConferenceSolutionKey = new ConferenceSolutionKey()
                        {
                            Type = "hangoutsMeet"
                        }
                    }
                }
            };

            // Insert the event
            EventsResource.InsertRequest requestInsert = service.Events.Insert(newEvent, "primary");
            requestInsert.ConferenceDataVersion = 1;
            Event createdEvent = await requestInsert.ExecuteAsync();

            return Ok(new { JoinUrl = createdEvent.HangoutLink });
        }
    }
}
