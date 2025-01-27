﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TransferLeaderRequest;
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
        private readonly IMapper _mapper;
        public LeaderTransferRequestController(ITransferLeaderRequestService transferLeaderRequestService, IMapper mapper)
        {
            _transferLeaderRequestService = transferLeaderRequestService;
            _mapper = mapper;
        }

        [HttpPost("leader-transfer")]
        [Authorize(Roles = RoleConstants.USER)]
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

        [HttpPut("leader-transfer/{requestId}/accept")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<IActionResult> LeaderTransferAfterMeetingConfirm([FromRoute] string projectId, [FromRoute] string requestId, [FromBody] LeaderTransferRequestDTO leaderTransferRequestDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _transferLeaderRequestService.TransferLeaderAfterMeeting(userId,projectId,requestId,leaderTransferRequestDTO);
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

        [HttpGet("leader-transfer")]
        public async Task<ActionResult<TransferLeaderRequestDetailDTO>> GetPendingTransferLeaderInAProject([FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var transferRequest = await _transferLeaderRequestService.GetPendingTransferLeaderRequest(userId,projectId);
                var response = _mapper.Map<TransferLeaderRequestDetailDTO>(transferRequest);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
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

        [HttpPut("leader-transfer/{requestId}/cancel")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<IActionResult> CancelTransferAfterMeetingConfirm([FromRoute] string projectId, [FromRoute] string requestId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _transferLeaderRequestService.CancelTransferLeaderRequest(userId, projectId, requestId);
                return Ok("Huỷ chuyển quyền thành công.");
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

        [HttpGet("leader-history")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<PaginationDTO<TransferLeaderHistoryResponseDTO>>> GetTransferLeaderHistoryInAProject([FromRoute] string projectId, [FromQuery] int page, [FromQuery] int size)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var historyResponse = await _transferLeaderRequestService.GetLeaderHistoryInTheProject(userId, projectId, page, size);
                return Ok(historyResponse);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
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

        [HttpGet("leader-transfer/{transferId}")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<TransferLeaderHistoryResponseDTO>> GetTransferLeaderHistoryInAProjectById([FromRoute] string projectId, [FromRoute] string transferId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var historyResponse = await _transferLeaderRequestService.GetLeaderHistoryInTheProjectById(userId,projectId,transferId);
                return Ok(historyResponse);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
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
