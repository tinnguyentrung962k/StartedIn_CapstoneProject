using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.DTOs.ResponseDTO.PayOs;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}")]
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
        [HttpPost("disbursements/{disbursementId}/payments")]
        public async Task<IActionResult> PaymentProccessing([FromRoute] string disbursementId, [FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var paymentResponse = await _payOsService.PaymentWithPayOs(userId,disbursementId,projectId);
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
        [HttpGet("disbursements/{disbursementId}/payment-info")]
        public async Task<IActionResult> GetPaymentStatus([FromRoute]string disbursementId, [FromRoute] string projectId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var paymentStatus = await _payOsService.GetPaymentStatus(userId,disbursementId,projectId);
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
        public async Task<IActionResult> HandleCancel([FromRoute]string projectId, [FromRoute] string disbursementId, [FromQuery] string apiKey)
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

        [HttpGet("disbursements/{disbursementId}/return")]
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

    }
}
