using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
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
        private readonly IContractService _contractService;
        public AdminController(
            IUserService userService, 
            IMapper mapper, 
            ILogger<AdminController> logger,
            IProjectService projectService,
            IContractService contractService)
        {
            _mapper = mapper;
            _userService = userService;
            _logger = logger;
            _projectService = projectService;
            _contractService = contractService;
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
            try
            {
                await _userService.ImportUsersFromExcel(formFile);
                return Ok("Hoàn thành tải file");
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpGet("projects")]
        [Authorize(Roles = RoleConstants.ADMIN)]
        public async Task<ActionResult<PaginationDTO<ProjectResponseDTO>>> GetAllProjects(int page, int size)
        {
            var response = await _projectService.GetAllProjectsForAdmin(page,size);
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

        [HttpPost("projects/{projectId}/internal-contract/{contractId}/download")]
        [Authorize(Roles = RoleConstants.ADMIN)]
        public async Task<ActionResult<DocumentDownLoadResponseDTO>> DownLoadInternalContract([FromRoute] string projectId, [FromRoute] string contractId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var downloadLink = await _contractService.DownloadContractForAdmin(projectId,contractId);
                return Ok(downloadLink);
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
    }
}
