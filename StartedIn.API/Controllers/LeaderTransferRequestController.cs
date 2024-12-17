using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [Route("api/projects/{projectId}")]
    [ApiController]
    public class LeaderTransferRequestController : ControllerBase
    {
        private readonly ITransferLeaderRequestService _transferLeaderRequestService;
        public LeaderTransferRequestController(ITransferLeaderRequestService transferLeaderRequestService)
        {
            _transferLeaderRequestService = transferLeaderRequestService;
        }

        [HttpPost("leader-transfer")]
        public async Task<IActionResult> CreateLeaderTransferRequest([FromRoute] string projectId, [FromBody] TerminationMeetingCreateDTO terminationMeetingCreateDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _transferLeaderRequestService.CreateLeaderTransferRequestInAProject(userId, projectId, terminationMeetingCreateDTO);
                return StatusCode(201, "Tạo yêu cầu và cuộc họp thành công");
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
                return StatusCode(500, MessageConstant.CreateFailed);
            }
        }
        [HttpPut("leader-transfer/{newLeaderId}")]
        public async Task<IActionResult> LeaderTransferAfterMeetingConfirm([FromRoute] string projectId, [FromRoute] string newLeaderId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _transferLeaderRequestService.TransferLeaderAfterMeeting(userId,projectId,newLeaderId);
                return Ok("Chuyển quyền thành công.");
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
                return StatusCode(500, MessageConstant.UpdateFailed);
            }
        }

    }
}
