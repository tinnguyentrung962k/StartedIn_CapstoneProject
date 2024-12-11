using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.TerminationRequest;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationRequest;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}")]
    public class TerminationRequestController : ControllerBase
    {
        private readonly ITerminationRequestService _terminationRequestService;
        private readonly IMapper _mapper;
        public TerminationRequestController(
            ITerminationRequestService terminationRequestService,
            IMapper mapper)
        {
            _terminationRequestService = terminationRequestService;
            _mapper = mapper;
        }

        [HttpPost("contracts/{contractId}/termination-request")]
        [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<IActionResult> CreateTerminationRequest([FromRoute] string projectId, [FromRoute] string contractId, [FromBody] TerminationRequestCreateDTO requestCreateDTO) 
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _terminationRequestService.CreateTerminationRequest(userId, projectId, contractId, requestCreateDTO);
                return StatusCode(201, "Gửi yêu cầu thành công");
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403,ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("termination-request")]
        [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<List<TerminationRequestResponseDTO>>> GetUserTermationRequestInProject([FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var requestList = await _terminationRequestService.GetTerminationRequestForUserInProject(userId, projectId);
                var response = _mapper.Map<List<TerminationRequestResponseDTO>>(requestList);
                return Ok(response);
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
                return StatusCode(500, ex.Message);
            }
        }

    }
}
