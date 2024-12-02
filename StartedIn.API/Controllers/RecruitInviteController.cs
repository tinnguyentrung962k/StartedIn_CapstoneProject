using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Project;
using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;
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
        [Authorize(Roles = RoleConstants.USER)]
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

        // Update Recruitment Info, if not existed, create new one

        // Update Recruitment Visibility

        // Update/Delete Recruitment Image
    }
}
