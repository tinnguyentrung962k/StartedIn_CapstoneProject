﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Project;
using StartedIn.CrossCutting.DTOs.RequestDTO.User;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Project;
using StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminController> _logger;
        private readonly IProjectService _projectService;
        public AdminController(
            IUserService userService, 
            IMapper mapper, 
            ILogger<AdminController> logger,
            IProjectService projectService)
        {
            _mapper = mapper;
            _userService = userService;
            _logger = logger;
            _projectService = projectService;
        }

        [HttpGet("users")]
        [Authorize(Roles = RoleConstants.ADMIN)]
        public async Task<ActionResult<PaginationDTO<FullProfileDTO>>> GetUserLists([FromQuery] UserAdminFilterDTO userAdminFilterDTO, [FromQuery] int page, int size)
        {
            try
            {
                var userResponse = await _userService.GetUsersListForAdmin(userAdminFilterDTO, page, size);
                return userResponse;
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex, "No user found.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting users list.");
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpPost("excel-import")]
        [Authorize(Roles = RoleConstants.ADMIN)]
        public async Task<IActionResult> ImportStudentExcelList(IFormFile formFile)
        {
            var result = await _userService.ImportUsersFromExcel(formFile);
            if (result.Success)
            {
                return Ok(new
                {
                    message = "Tải file thành công.",
                    users = result.Data.Select(user => new {user.Email})
                });
            }
            else
            {
                return BadRequest(new
                {
                    message = "Lỗi xảy ra trong file",
                    errors = result.Errors
                });
            }
        }

        [HttpGet("projects")]
        [Authorize(Roles = RoleConstants.ADMIN)]
        public async Task<ActionResult<PaginationDTO<ProjectResponseDTO>>> GetAllProjects([FromQuery] ProjectAdminFilterDTO projectAdminFilterDTO, [FromQuery] int page, [FromQuery] int size)
        {
            var response = await _projectService.GetAllProjectsForAdmin(projectAdminFilterDTO, page, size);
            return Ok(response);
        }

        [HttpPut("projects/{projectId}/activate")]
        [Authorize(Roles = RoleConstants.ADMIN)]
        public async Task<ActionResult<ProjectResponseDTO>> ActivateProject([FromRoute] string projectId)
        {
            try
            {
                var project = await _projectService.ActivateProject(projectId);
                var response = _mapper.Map<ProjectResponseDTO>(project);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return StatusCode(400, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpGet("projects/{projectId}")]
        [Authorize(Roles = RoleConstants.ADMIN)]
        public async Task<ActionResult<ProjectDetailForAdminDTO>> GetProjectDetail([FromRoute] string projectId)
        {
            try
            {
                var project = await _projectService.GetProjectById(projectId);
                var response = _mapper.Map<ProjectDetailForAdminDTO>(project);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("users/{userId}/toggle")]
        [Authorize(Roles = RoleConstants.ADMIN)]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            try
            {
                await _userService.ToggleUserStatus(userId);
                return Ok("Cập nhật trạng thái thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }
    }
}
