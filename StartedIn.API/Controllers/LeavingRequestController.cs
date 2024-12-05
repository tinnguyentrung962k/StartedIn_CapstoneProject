using AutoMapper;
using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.LeavingRequest;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.LeavingRequest;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [Route("api/projects/{projectId}")]
    [ApiController]
    public class LeavingRequestController : ControllerBase
    {
        private ILeavingRequestService _leavingRequestService;
        private IMapper _mapper;
        public LeavingRequestController(
            ILeavingRequestService leavingRequestService,
            IMapper mapper
        )
        {
            _leavingRequestService = leavingRequestService;
            _mapper = mapper;
        }

        [HttpGet("leaving-requests/{requestId}")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.MENTOR + "," + RoleConstants.INVESTOR)]
        public async Task<ActionResult<LeavingRequestResponseDTO>> GetLeavingRequestById([FromRoute] string projectId, [FromRoute] string requestId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var request = await _leavingRequestService.GetLeavingRequestById(userId, projectId, requestId);
                var response = _mapper.Map<LeavingRequestResponseDTO>(request);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex);
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

        [HttpPost("leaving-requests")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.MENTOR + "," + RoleConstants.INVESTOR)]
        public async Task<ActionResult<LeavingRequestResponseDTO>> CreateLeavingRequest([FromRoute] string projectId, [FromBody] LeavingRequestCreateDTO leavingRequestCreateDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var request = await _leavingRequestService.CreateLeavingRequest(userId, projectId, leavingRequestCreateDTO);
                var response = _mapper.Map<LeavingRequestResponseDTO>(request);
                return CreatedAtAction(nameof(GetLeavingRequestById), new { projectId, requestId = response.Id }, response);

            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ExistedRecordException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("leaving-requests/{requestId}/accept")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<IActionResult> AcceptLeavingRequest([FromRoute] string projectId, [FromRoute] string requestId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _leavingRequestService.AcceptLeavingRequest(userId, projectId, requestId);
                return Ok("Chấp nhận đề nghị thành công");
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (InvalidInputException ex)
            {
                return BadRequest(ex.Message);
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

        [HttpPut("leaving-requests/{requestId}/reject")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<IActionResult> RejectLeavingRequest([FromRoute] string projectId, [FromRoute] string requestId, IFormFile file)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _leavingRequestService.RejectLeavingRequest(userId,projectId,requestId);
                return Ok("Từ chối đề nghị thành công");
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
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("leaving-requests")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<List<LeavingRequestResponseDTO>>> FilterLeavingRequestInProject([FromRoute] string projectId)
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var response = await _leavingRequestService.FilterLeavingRequestForLeader(userId, projectId);
                return Ok(response);
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
                return StatusCode(500, ex.Message);
            }
        }
    }
}
