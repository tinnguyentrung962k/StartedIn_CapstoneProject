using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.EquityShare;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ShareEquity;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}")]
    public class ShareEquityController : ControllerBase
    {
        private readonly IMapper _mapper;
        private ILogger<ShareEquityController> _logger;
        private readonly IShareEquityService _shareEquityService;
        public ShareEquityController(
            IMapper mapper, 
            ILogger<ShareEquityController> logger, 
            IShareEquityService shareEquityService
        )
        {
            _mapper = mapper;
            _logger = logger;
            _shareEquityService = shareEquityService;
        }
        [HttpGet("share-equities")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<List<ShareEquitiesOfMemberInAProject>>> GetShareEquitiesInAProject([FromRoute] string projectId, [FromQuery] EquityShareFilterDTO equityShareFilterDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var shareEquityList = await _shareEquityService.GetShareEquityOfAllMembersInAProject(userId, projectId, equityShareFilterDTO);
                var responseShareEquity = _mapper.Map<List<ShareEquitiesOfMemberInAProject>>(shareEquityList);
                return Ok(responseShareEquity);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
