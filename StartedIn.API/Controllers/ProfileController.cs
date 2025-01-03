using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.User;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProfileController> _logger;
        public ProfileController(IUserService userService, IMapper mapper, ILogger<ProfileController> logger)
        {
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        // Lấy các thông tin cần để hiển thị ở profile header
        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<HeaderProfileDTO>> GetCurrentUserProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var queryUser = await _userService.GetUserWithUserRolesById(userId);
            if (queryUser == null)
            {
                return BadRequest(MessageConstant.NotFoundUserError);
            }
            var profileDto = _mapper.Map<HeaderProfileDTO>(queryUser);
            return Ok(profileDto);
        }

        // lấy full cho page profile
        [Authorize]
        [HttpGet("full-profile")]
        public async Task<ActionResult<FullProfileDTO>> GetCurrentUserFullProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var queryUser = await _userService.GetUserWithId(userId);
            if (queryUser == null)
            {
                return BadRequest(MessageConstant.NotFoundUserError);
            }

            var fullProfileDto = _mapper.Map<FullProfileDTO>(queryUser);
            return Ok(fullProfileDto);
        }

        [Authorize]
        [HttpPost("profile-picture")]
        public async Task<ActionResult<FullProfileDTO>> UploadAvatar(IFormFile avatar)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (userId == null)
            {
                return BadRequest("Không tìm thấy người dùng");
            }
            try
            {
                var user = await _userService.UpdateAvatar(avatar, userId);
                var responseUserProfile = _mapper.Map<FullProfileDTO>(user);
                return Ok(responseUserProfile);
            }
            catch (Exception ex)
            {
                return BadRequest("Cập nhật ảnh đại diện thất bại");
            }
        }

        [Authorize]
        [HttpPost("cover-photo")]
        public async Task<ActionResult<FullProfileDTO>> UploadCoverPhoto(IFormFile coverPhoto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (userId == null)
            {
                return BadRequest("Không tìm thấy người dùng");
            }
            try
            {
                var user = await _userService.UpdateCoverPhoto(coverPhoto, userId);
                var responseUserProfile = _mapper.Map<FullProfileDTO>(user);
                return Ok(responseUserProfile);
            }
            catch (Exception ex)
            {
                return BadRequest("Cập nhật ảnh bìa thất bại");
            }
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<FullProfileDTO>> EditProfile([FromBody] UpdateProfileDTO updateProfileDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (userId == null)
            {
                return BadRequest("Không tìm thấy người dùng");
            }

            try
            {
                var user = await _userService.UpdateProfile(_mapper.Map<User>(updateProfileDto), userId);
                var updatedUser = _mapper.Map<FullProfileDTO>(user);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return BadRequest("Cập nhật người dùng không thành công");
            }
        }
    }
}
