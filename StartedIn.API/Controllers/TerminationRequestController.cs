using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
using StartedIn.CrossCutting.DTOs.RequestDTO.TerminationRequest;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationRequest;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}")]
    public class TerminationRequestController : ControllerBase
    {
        private readonly ITerminationRequestService _terminationRequestService;
        public TerminationRequestController(
            ITerminationRequestService terminationRequestService)
        {
            _terminationRequestService = terminationRequestService;
        }

        [HttpPost("termination-requests")]
        [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<IActionResult> CreateTerminationRequest([FromRoute] string projectId, [FromBody] TerminationRequestCreateDTO requestCreateDTO) 
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _terminationRequestService.CreateTerminationRequest(userId, projectId, requestCreateDTO);
                return StatusCode(201, "Gửi yêu cầu thành công");
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403,ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("received-termination-requests")]
        [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<List<TerminationRequestReceivedResponseDTO>>> GetUserReceivedTermationRequestInProject([FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var requestList = await _terminationRequestService.GetTerminationRequestForToUserInProject(userId, projectId);
                return Ok(requestList);
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

        [HttpGet("sent-termination-requests")]
        [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<List<TerminationRequestReceivedResponseDTO>>> GetUserSentTermationRequestInProject([FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var requestList = await _terminationRequestService.GetTerminationRequestForFromUserInProject(userId, projectId);
                return Ok(requestList);
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

        [HttpPut("termination-requests/{requestId}/accept")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<IActionResult> AcceptTermationRequest([FromRoute] string projectId, [FromRoute] string requestId, [FromBody] TerminationMeetingCreateDTO terminationMeetingCreateDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _terminationRequestService.AcceptTerminationRequest(userId, projectId, requestId, terminationMeetingCreateDTO);
                return Ok("Chấp nhận yêu cầu thành công");
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (UpdateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("termination-requests/{requestId}/reject")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<IActionResult> RejectTermationRequest([FromRoute] string projectId, [FromRoute] string requestId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _terminationRequestService.RejectTerminationRequest(userId, projectId, requestId);
                return Ok("Từ chối yêu cầu thành công");
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (UpdateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
