using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationConfirmation;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationRequest;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [Route("api/projects/{projectId}")]
    [ApiController]
    public class TerminationConfirmationController : ControllerBase
    {
        private readonly ITerminationConfirmationService _terminationConfirmService;
        public TerminationConfirmationController(ITerminationConfirmationService terminationConfirmationService)
        {
            _terminationConfirmService = terminationConfirmationService;
        }

        [HttpGet("termination-confirmations")]
        [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<List<TerminationConfirmationResponseDTO>>> GetUserTermationConfirmationInProject([FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var confirmList = await _terminationConfirmService.GetTerminationConfirmationForUserInProject(userId, projectId);
                return Ok(confirmList);
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

        [HttpPut("termination-confirmations/{confirmId}/accept")]
        [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<IActionResult> AcceptATerminationRequest([FromRoute] string projectId, [FromRoute] string confirmId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _terminationConfirmService.AcceptTerminationRequest(userId, projectId, confirmId);
                return Ok("Đã chấp nhận thành công");
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
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("termination-confirmations/{confirmId}/reject")]
        [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<IActionResult> RejectATerminationRequest([FromRoute] string projectId, [FromRoute] string confirmId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _terminationConfirmService.RejectTerminationRequest(userId, projectId, confirmId);
                return Ok("Đã từ chối thành công");
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
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("termination-confirmations/{confirmId}/request-detail")]
        [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<TerminationRequestDetailDTO>> GetTerminationDetail([FromRoute] string projectId, [FromRoute] string confirmId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var response = await _terminationConfirmService.GetContractTerminationDetailByConfirmId(userId, projectId, confirmId);
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
            catch (UnauthorizedAccessException ex)
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
