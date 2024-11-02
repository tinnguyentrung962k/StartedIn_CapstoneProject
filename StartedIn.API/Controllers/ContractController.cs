using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.SignNowWebhookRequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;
using System.Collections.Generic;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IMapper _mapper;
        private readonly ILogger<ContractController> _logger;
        private readonly ISignNowService _signNowService;
        public ContractController(IContractService contractService, IMapper mapper, ILogger<ContractController> logger, ISignNowService signNowService)
        {
            _contractService = contractService;
            _mapper = mapper;
            _logger = logger;
            _signNowService = signNowService;
        }
        [HttpPost("/investment-contract")]
        [Authorize]
        public async Task<ActionResult<ContractResponseDTO>> CreateAnInvestmentContract([FromBody] InvestmentContractCreateDTO investmentContractCreateDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            try
            {
                var contract = await _contractService.CreateInvestmentContract(userId, investmentContractCreateDTO);
                var responseContract = _mapper.Map<ContractResponseDTO>(contract);
                return CreatedAtAction(nameof(GetContractById), new { contractId = responseContract.Id }, responseContract);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating contract");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpPost("/contract/send-invite/{contractId}")]
        [Authorize]
        public async Task<ActionResult<ContractResponseDTO>> SendInviteForContract([FromRoute] string contractId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            try
            {
                var contract = await _contractService.SendSigningInvitationForContract(userId, contractId);
                var responseContract = _mapper.Map<ContractResponseDTO>(contract);

                return Ok(responseContract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating contract");
                return StatusCode(500, "Lỗi server");
            }
        }
        [HttpGet("/contract/user-contract/project/{projectId}")]
        [Authorize]
        public async Task<ActionResult<List<ContractResponseDTO>>> GetPersonalContractsInAProject([FromRoute] string projectId, [FromQuery] int pageIndex, int pageSize)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            try
            {
                var contracts = await _contractService.GetContractsByUserIdInAProject(userId, projectId, pageIndex, pageSize);
                var responseContract = _mapper.Map<List<ContractResponseDTO>>(contracts);
                return Ok(responseContract);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating contract");
                return StatusCode(500, "Lỗi server");
            }
        }
        [HttpGet("/contract/{contractId}")]
        [Authorize]
        public async Task<ActionResult<ContractResponseDTO>> GetContractById([FromRoute] string contractId)
        {
            try
            {
                var contract = await _contractService.GetContractByContractId(contractId);
                var responseContract = _mapper.Map<ContractResponseDTO>(contract);
                return Ok(responseContract);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("/api/contract/valid-contract/{contractId}")]
        public async Task<IActionResult> ValidAcontract([FromRoute] string contractId)
        {
            try
            {
                var contract = await _contractService.ValidateContractOnSignedAsync(contractId);
                return Ok("Cập nhật hợp đồng thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi Cập nhật");
            }
        }

    }
        
}
