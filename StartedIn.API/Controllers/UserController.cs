using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, IMapper mapper, ILogger<UserController> logger)
        {
            _mapper = mapper;
            _userService = userService;
            _logger = logger;
        }
        [HttpGet("users")]
        [Authorize(Roles = RoleConstants.ADMIN)]
        public async Task<ActionResult<IEnumerable<FullProfileDTO>>> GetUserLists([FromQuery] int pageIndex, int pageSize)
        {
            try
            {
                var userList = await _userService.GetUsersList(pageIndex, pageSize);
                var responseUserList = _mapper.Map<List<FullProfileDTO>>(userList);
                return responseUserList;
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex, "No user found.");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting users list.");
                return StatusCode(500, ex.Message);
            }

        }
        [HttpPost("users/import-user-excel")]
        [Authorize(Roles = RoleConstants.ADMIN)]
        public async Task<IActionResult> ImportStudentExcelList(IFormFile formFile)
        {
            try
            {
                await _userService.ImportUsersFromExcel(formFile);
                return Ok("Hoàn thành tải file");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
