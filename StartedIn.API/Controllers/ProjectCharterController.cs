using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;
using CrossCutting.Exceptions;
using StartedIn.CrossCutting.DTOs.RequestDTO.ProjectCharter;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ProjectCharter;
using System.ComponentModel.DataAnnotations;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/project-charters")]
    public class ProjectCharterController : ControllerBase
    {
        private readonly IProjectCharterService _projectCharterService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProjectCharterController> _logger;

        public ProjectCharterController(IProjectCharterService projectCharterService, IMapper mapper, ILogger<ProjectCharterController> logger)
        {
            _projectCharterService = projectCharterService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<ProjectCharterResponseDTO>> CreateNewProjectCharter([FromRoute] string projectId, [FromBody]ProjectCharterCreateDTO projectCharterCreateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var projectCharter =
                    await _projectCharterService.CreateNewProjectCharter(userId, projectId, projectCharterCreateDto);
                var projectCharterResponse = _mapper.Map<ProjectCharterResponseDTO>(projectCharter);
                return CreatedAtAction(nameof(GetProjectCharterByProjectId), new { projectId },
                    projectCharterResponse);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                _logger.LogError(ex, "Unauthorized Role");
                return StatusCode(403, ex.Message);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex, "User or project is not found");
                return BadRequest(ex.Message);
            }
            catch (ExistedRecordException ex)
            {
                _logger.LogError(ex, "Charter existed for project");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating new project charter.");

                return StatusCode(500, MessageConstant.InternalServerError);

            }
        }

        [HttpGet]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR)]
        public async Task<ActionResult<ProjectCharterResponseDTO>> GetProjectCharterByProjectId([FromRoute] string projectId)
        {
            try
            {
                var projectCharter = _mapper.Map<ProjectCharterResponseDTO>(await _projectCharterService.GetProjectCharterByProjectId(projectId));
                return Ok(projectCharter);
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
        
        [HttpPut]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<ProjectCharterResponseDTO>> EditProjectCharter([FromRoute] string projectId, [FromBody] EditProjectCharterDTO editProjectCharterDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var responseProjectCharter = _mapper.Map<ProjectCharterResponseDTO>(await _projectCharterService.UpdateProjectCharterInfo(userId,projectId,editProjectCharterDto));
                return Ok(responseProjectCharter);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                _logger.LogError(ex, "Unauthorized Role");
                return StatusCode(403, ex.Message);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnmatchedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(MessageConstant.UpdateFailed);
            }
        }

    }
}
