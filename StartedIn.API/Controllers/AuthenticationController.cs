﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;
        private readonly IMapper _mapper;

        public AccountController(IUserService accountService, ILogger<AccountController> logger, IMapper mapper)
        {
            _userService = accountService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            try
            {
                LoginResponseDTO res = await _userService.Login(loginDto.Email, loginDto.Password);
                return Ok(res);
            }
            catch (InvalidLoginException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotActivateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while login");
                return StatusCode(500, "Lỗi đăng nhập xảy ra");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            try
            {
                await _userService.Register(_mapper.Map<User>(registerDto), registerDto.Password);
                return Ok("Tạo tài khoản thành công");
            }
            catch (ExistedEmailException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while register");
                return StatusCode(500, "Lỗi tạo tài khoản");
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDTO refreshTokenDto)
        {
            try
            {
                var res = await _userService.Refresh(refreshTokenDto.RefreshToken);
                refreshTokenDto.AccessToken = res;
                return Ok(refreshTokenDto);
            }
            catch (NotFoundException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server Error");
                return StatusCode(500, "Lỗi server");
            }

        }

        [Authorize]
        [HttpDelete("revoke")]
        public async Task<IActionResult> Revoke()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (userId == null)
                {
                    return Unauthorized();
                }
                await _userService.Revoke(userId);
                return Ok("Revoke Successfully !");
            }

            catch (Exception ex)
            {
                return BadRequest("Invalid refresh token");
            }


        }
        [HttpGet("activate-user/{userId}")]
        public async Task<IActionResult> ActivateUser(string userId)
        {
            try
            {
                await _userService.ActivateUser(userId);
                return Ok("Kích hoạt thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi kích hoạt");
            }
        }
        [HttpPost("import-user-excel")]
        public async Task<IActionResult> ImportStudentExcelList(IFormFile formFile)
        {
            try
            {
                await _userService.ImportUsersFromExcel(formFile);
                return Ok("Hoàn thành tải file");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi tải file");
            }
        }

        [HttpPost("request-reset-password")]
        public async Task<IActionResult> RequestResetPasswordLink(string email)
        {
            try
            {
                await _userService.RequestResetPassword(email);
                return Ok("Link đã được gửi đến email");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            try
            {
                await _userService.ResetPassword(resetPasswordDTO);
                return Ok("Reset Password thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server");
            }
        }

    }
}
