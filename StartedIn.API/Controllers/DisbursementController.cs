using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.ResponseDTO.PayOs;
using StartedIn.CrossCutting.Exceptions;
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
        public DisbursementController(IPayOsService payOsService, ILogger<DisbursementController> logger, IDisbursementService disbursementService)
        {
            _logger = logger;
            _payOsService = payOsService;
            _disbursementService = disbursementService;
        }
        [HttpPost("projects/{projectId}/disbursements/{disbursementId}/payments")]
        public async Task<IActionResult> PaymentProccessing([FromRoute] string disbursementId, [FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var paymentResponse = await _payOsService.PaymentWithPayOs(userId, disbursementId, projectId);
                return Ok(paymentResponse);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Server Error");
            }
        }
        [HttpGet("projects/{projectId}/disbursements/{disbursementId}/payment-info")]
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

        [HttpGet("projects/{projectId}/disbursements/{disbursementId}/cancel")]
        public async Task<IActionResult> HandleCancel([FromRoute] string projectId, [FromRoute] string disbursementId, [FromQuery] string apiKey)
        {
            try
            {
                await _disbursementService.CancelPayment(disbursementId, projectId, apiKey);
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

        [HttpGet("projects/{projectId}/disbursements/{disbursementId}/return")]
        public async Task<IActionResult> HandleReturn([FromRoute] string projectId, [FromRoute] string disbursementId, [FromQuery] string apiKey)
        {
            try
            {
                await _disbursementService.FinishedTheTransaction(disbursementId, projectId, apiKey);
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
        public async Task<ActionResult<PaginationDTO<DisbursementForLeaderInProjectResponseDTO>>> GetListDisbursementForInvestorInMenu(
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

    }
}
