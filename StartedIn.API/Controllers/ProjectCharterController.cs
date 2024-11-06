﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

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
                return CreatedAtAction(nameof(GetProjectCharterByCharterId), new { id = projectCharterResponse.Id },
                    projectCharterResponse);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                _logger.LogError(ex, "Unauthorized Role");
                return StatusCode(403, ex.Message);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex, "User or project is not found");
                return BadRequest(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating new project charter.");

                return StatusCode(500, MessageConstant.InternalServerError);

            }
        }

        [HttpGet("/api/project-charters/{id}")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR)]
        public async Task<ActionResult<ProjectCharterResponseDTO>> GetProjectCharterByCharterId([FromRoute] string id)
        {
            try
            {
                var projectCharter = _mapper.Map<ProjectCharterResponseDTO>(await _projectCharterService.GetProjectCharterByCharterId(id));
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

    }
}
