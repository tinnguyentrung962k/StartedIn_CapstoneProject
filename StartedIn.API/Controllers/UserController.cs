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
    [Route("api/users")]
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

        [HttpGet("{userId}")]
        [Authorize]
        public async Task<ActionResult<FullProfileDTO>> GetUserById(string userId)
        {
            try
            {
                var user = await _userService.GetUserWithId(userId);
                return Ok(_mapper.Map<FullProfileDTO>(user));
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex, MessageConstant.NotFoundUserError);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting user.");
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }
    }
}
