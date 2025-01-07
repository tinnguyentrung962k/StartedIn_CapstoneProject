using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Project;
using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
using StartedIn.CrossCutting.DTOs.ResponseDTO.RecruitInvite;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;
using System;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}")]
    public class RecruitInviteController : ControllerBase
    {
        private readonly IRecruitInviteService _recruitInviteService;
        private readonly IMapper _mapper;

        public RecruitInviteController(IRecruitInviteService recruitInviteService, IMapper mapper)
        {
            _recruitInviteService = recruitInviteService;
            _mapper = mapper;
        }


        [HttpPost("invite")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<IActionResult> SendInvitationToTeam([FromBody] List<string> inviteUserEmails, [FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _recruitInviteService.SendJoinProjectInvitation(userId, inviteUserEmails, projectId);
                return Ok("Gửi lời mời gia nhập thành công");
            }
            catch (InviteException ex)
            {
                return BadRequest(ex.Message);
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

        [HttpGet("invite-overview")]
        public async Task<IActionResult> GetProjectOverview([FromRoute] string projectId)
        {
            try
            {
                var projectOverview = await _recruitInviteService.GetProjectInviteOverview(projectId);
                return Ok(projectOverview);
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

        [HttpPost("add-user/{userId}/{roleInTeam}")]
        public async Task<IActionResult> AddAUserToProject([FromRoute] string projectId, [FromRoute] string userId, [FromRoute] RoleInTeam roleInTeam)
        {
            try
            {
                await _recruitInviteService.AddUserToProject(projectId, userId, roleInTeam);
                return Ok("Thành viên đã được thêm vào nhóm.");
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InviteException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpPost("accept-invite")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<IActionResult> AcceptInvitation([FromRoute] string projectId, [FromBody] AcceptInviteDTO acceptInviteDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _recruitInviteService.AcceptProjectInvitation(userId, projectId, acceptInviteDTO);
                return Ok("Bạn đã chấp nhận lời mời!");
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InviteException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }


        [HttpPost("join")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR)]
        public async Task<IActionResult> JoinAProject([FromRoute] string projectId, RoleInTeam roleInTeam)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _recruitInviteService.AddUserToProject(projectId, userId, roleInTeam);
                return Ok("Bạn đã được tham gia dự án!");
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InviteException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        // Create recruitment CV info
        [HttpPost("recruitments/{recruitmentId}/apply")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<IActionResult> ApplyRecruitment([FromRoute] string projectId, [FromRoute] string recruitmentId, [FromForm] ApplyRecruitmentDTO applyRecruitmentDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _recruitInviteService.ApplyRecruitment(userId, projectId, recruitmentId, applyRecruitmentDTO.cvFiles);
                return Ok("Ứng tuyển thành công!");
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InviteException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        // Get list of application of a project, so leader can reject or accept the application after reviewing them
        [HttpGet("applications")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<IEnumerable<ApplicationDTO>>> GetApplications([FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var applications = await _recruitInviteService.GetApplicationsOfProject(userId, projectId);
                var data = _mapper.Map<IEnumerable<ApplicationDTO>>(applications);
                return Ok(data);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InviteException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        // Accept an application and add that user into the project
        [HttpPatch("applications/{applicationId}/accept")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<IActionResult> AcceptApplication([FromRoute] string projectId, [FromRoute] string applicationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _recruitInviteService.AcceptApplication(userId, projectId, applicationId);
                return Ok("Chấp nhận ứng tuyển thành công!");
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InviteException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Reject an application
        [HttpPatch("applications/{applicationId}/reject")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<IActionResult> RejectApplication([FromRoute] string projectId, [FromRoute] string applicationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _recruitInviteService.RejectApplication(userId, projectId, applicationId);
                return Ok("Từ chối ứng tuyển thành công!");
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InviteException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpGet("pending-invitations")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<List<UserInvitationInProjectDTO>>> GetPendingInvitationInProject([FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var invitations = await _recruitInviteService.GetPendingInvitationOfProject(userId,projectId);
                var response = _mapper.Map<List<UserInvitationInProjectDTO>>(invitations);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403,ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }
    }
}
