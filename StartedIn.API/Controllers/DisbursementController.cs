using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.API.Attributes;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.ResponseDTO.PayOs;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class DisbursementController : ControllerBase
    {
        private readonly IPayOsService _payOsService;
        private readonly ILogger<DisbursementController> _logger;
        private readonly IDisbursementService _disbursementService;
        private readonly IMapper _mapper;
        public DisbursementController(IPayOsService payOsService, ILogger<DisbursementController> logger, IDisbursementService disbursementService, IMapper mapper)
        {
            _logger = logger;
            _payOsService = payOsService;
            _disbursementService = disbursementService;
            _mapper = mapper;
        }
        [HttpPost("disbursements/{disbursementId}/payments")]
        [Authorize(Roles = RoleConstants.INVESTOR)]
        public async Task<IActionResult> PaymentProccessing([FromRoute] string disbursementId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var paymentResponse = await _payOsService.PaymentWithPayOs(userId, disbursementId);
                return Ok(paymentResponse);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        [HttpGet("projects/{projectId}/disbursements/{disbursementId}/payment-info")]
        [Authorize(Roles = RoleConstants.INVESTOR + "," + RoleConstants.USER)]
        public async Task<IActionResult> GetPaymentStatus([FromRoute] string disbursementId, [FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var paymentStatus = await _payOsService.GetPaymentStatus(userId, disbursementId, projectId);
                return Ok(paymentStatus);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("disbursements/{disbursementId}/cancel")]
        public async Task<IActionResult> HandleCancel([FromRoute] string disbursementId, [FromQuery] string apiKey)
        {
            try
            {
                await _disbursementService.CancelPayment(disbursementId, apiKey);
                var redirectUrl = "https://www.startedin.net/payment-fail";
                return Redirect(redirectUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("disbursements/{disbursementId}/return")]
        public async Task<IActionResult> HandleReturn([FromRoute] string disbursementId, [FromQuery] string apiKey)
        {
            try
            {
                await _disbursementService.FinishedTheTransaction(disbursementId, apiKey);
                var redirectUrl = "https://www.startedin.net/payment-success";
                return Redirect(redirectUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("projects/{projectId}/disbursements")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<PaginationDTO<DisbursementForLeaderInProjectResponseDTO>>> GetListDisbursementInProjectForLeader(
            [FromRoute] string projectId, 
            [FromQuery] DisbursementFilterInProjectDTO disbursementFilterDTO,
            [FromQuery] int page,
            [FromQuery] int size)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var disbursementPaginationList = await _disbursementService.GetDisbursementListForLeaderInAProject(userId, projectId, disbursementFilterDTO, size, page);
                return Ok(disbursementPaginationList);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError + ex.Message);
            }
        }
        [HttpGet("disbursements")]
        [Authorize(Roles = RoleConstants.INVESTOR)]
        public async Task<ActionResult<PaginationDTO<DisbursementForInvestorInInvestorMenuResponseDTO>>> GetListDisbursementForInvestorInMenu(
            [FromQuery] DisbursementFilterInvestorMenuDTO disbursementFilterDTO,
            [FromQuery] int page,
            [FromQuery] int size)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var disbursementPaginationList = await _disbursementService.GetDisbursementListForInvestorInMenu(userId, disbursementFilterDTO, size, page);
                return Ok(disbursementPaginationList);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError + ex.Message);
            }
        }
        [HttpPut("disbursements/{disbursementId}/reject")]
        [Authorize(Roles = RoleConstants.INVESTOR)]
        public async Task<IActionResult> RejectADisbursement([FromRoute] string disbursementId, [FromBody] DisbursementRejectDTO disbursementRejectDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _disbursementService.RejectADisbursement(userId, disbursementId, disbursementRejectDTO);
                return Ok("Từ chối yêu cầu thành công");
            }
            catch (UnmatchedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UpdateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError + ex.Message);
            }
        }
        [HttpPut("projects/{projectId}/disbursements/{disbursementId}/confirm")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<IActionResult> ConfirmADisbursement([FromRoute] string projectId, [FromRoute] string disbursementId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _disbursementService.DisbursementConfirmation(userId, projectId, disbursementId);
                return Ok("Xác nhận mốc giải ngân thành công.");
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (UnmatchedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UpdateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError + ex.Message);
            }
        }
        [HttpPost("disbursements/{disbursementId}/accept")]
        [Authorize(Roles = RoleConstants.INVESTOR)]
        public async Task<IActionResult> AcceptAndUploadEvidenceForADisbursement([FromRoute] string disbursementId, [FromForm] List<IFormFile> evidenceFiles)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _disbursementService.AcceptDisbursement(userId, disbursementId, evidenceFiles);
                return Ok("Chấp nhận thành công.");
            }
            catch (UnmatchedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UploadFileException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError + ex.Message);
            }
        }
        [HttpGet("projects/{projectId}/disbursements/{disbursementId}")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<DisbursementDetailForLeaderInProjectResponseDTO>> GetADisbursementDetailInProjectForLeader([FromRoute] string projectId, [FromRoute] string disbursementId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var disbursement = await _disbursementService.GetADisbursementDetailForLeader(userId, projectId, disbursementId);
                var response = _mapper.Map<DisbursementDetailForLeaderInProjectResponseDTO>(disbursement);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnmatchedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError + ex.Message);
            }
        }

        [HttpGet("disbursements/{disbursementId}")]
        [Authorize(Roles = RoleConstants.INVESTOR)]
        public async Task<ActionResult<DisbursementDetailForInvestorResponseDTO>> GetADisbursementDetailInMenuForInvestor([FromRoute] string disbursementId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var disbursement = await _disbursementService.GetADisbursementDetailInvestor(userId, disbursementId);
                var response = _mapper.Map<DisbursementDetailForInvestorResponseDTO>(disbursement);
                return Ok(response);
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
                return StatusCode(500, MessageConstant.InternalServerError + ex.Message);
            }
        }

        [HttpGet("projects/{projectId}/disbursements/project-info")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.MENTOR), RequireProjectAccess]
        public async Task<ActionResult<List<DisbursementOverviewOfProject>>> GetDisbursementOverviewOfProject([FromRoute] string projectId)
        {
            try
            {
                var currentMonthInfo = await _disbursementService.GetADisbursementTotalInAMonth(projectId, DateTime.Now);
                var nextMonthInfo = await _disbursementService.GetADisbursementTotalInAMonth(projectId, DateTime.Now.AddMonths(1));

                var result = new List<DisbursementOverviewOfProject>
                {
                    currentMonthInfo,
                    nextMonthInfo
                };

                return Ok(result);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("disbursements/investor-info")]
        [Authorize(Roles = RoleConstants.INVESTOR)]
        public async Task<ActionResult<List<DisbursementOverviewOfProjectForInvestor>>> GetDisbursementOverviewOfProjectForInvestor([FromQuery] int page, int size)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var result = await _disbursementService.GetADisbursementOverviewForInvestor(userId,page,size);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("disbursements/investor-overview")]
        [Authorize(Roles = RoleConstants.INVESTOR)]
        public async Task<ActionResult<InvestorDisbursementOverviewDTO>> GetInvestorDisbursementOverview()
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var result = await _disbursementService.GetInvestorDisbursementOverviewAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
