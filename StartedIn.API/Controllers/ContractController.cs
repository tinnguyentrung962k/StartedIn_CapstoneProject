using AutoMapper;
using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.SignNowWebhookRequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;
using System.Collections.Generic;
using System.Security.Claims;
using StartedIn.CrossCutting.Constants;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}")]
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
        
        [HttpPost("investment-contracts")]
        [Authorize]
        public async Task<ActionResult<ContractResponseDTO>> CreateAnInvestmentContract([FromRoute] string projectId, [FromBody] InvestmentContractCreateDTO investmentContractCreateDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.CreateInvestmentContract(userId, projectId, investmentContractCreateDTO);
                var responseContract = _mapper.Map<ContractResponseDTO>(contract);
                return CreatedAtAction(nameof(GetContractById), new { projectId, contractId = responseContract.Id }, responseContract);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (ExistedRecordException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating contract");

                return StatusCode(500, MessageConstant.InternalServerError);

            }
        }
        [HttpPut("investment-contracts/{contractId}")]
        [Authorize]
        public async Task<ActionResult<ContractResponseDTO>> UpdateInvestmentContract([FromRoute] string projectId, [FromRoute] string contractId, [FromBody] InvestmentContractUpdateDTO investmentContractUpdateDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.UpdateInvestmentContract(userId,projectId,contractId,investmentContractUpdateDTO);
                var responseContract = _mapper.Map<ContractResponseDTO>(contract);
                return Ok(responseContract);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UpdateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating contract");
                return StatusCode(500, MessageConstant.InternalServerError);

            }
        }
        [HttpGet("investment-contracts/{contractId}")]
        [Authorize]
        public async Task<ActionResult<ContractDetailResponseDTO>> GetInvestmentContractDetail([FromRoute]string projectId, [FromRoute]string contractId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.GetContractByContractId(userId,contractId,projectId);
                var responseContract = _mapper.Map<ContractDetailResponseDTO>(contract);
                return Ok(responseContract);
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
                return StatusCode(500, MessageConstant.InternalServerError); 
            }
        }

        [HttpPost("contracts/{contractId}/invite")]

        [Authorize]
        public async Task<ActionResult<ContractResponseDTO>> SendInviteForContract([FromRoute]string projectId, [FromRoute] string contractId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.SendSigningInvitationForContract(projectId, userId, contractId);
                var responseContract = _mapper.Map<ContractResponseDTO>(contract);

                return Ok(responseContract);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (UnmatchedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating contract");
                return StatusCode(500, MessageConstant.InternalServerError);

            }
        }

        [HttpGet("contracts/{contractId}")]
        [Authorize]
        public async Task<ActionResult<ContractResponseDTO>> GetContractById([FromRoute]string projectId, [FromRoute] string contractId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.GetContractByContractId(userId, contractId, projectId);
                var responseContract = _mapper.Map<ContractResponseDTO>(contract);
                return Ok(responseContract);
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
                return StatusCode(500, MessageConstant.InternalServerError); ;
            }
        }

        [HttpPost("contracts/{contractId}/validate")]
        public async Task<IActionResult> ValidAcontract([FromRoute]string projectId, [FromRoute] string contractId)
        {
            try
            {
                var contract = await _contractService.ValidateContractOnSignedAsync(contractId, projectId);
                return Ok("Cập nhật hợp đồng thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpPost("contracts/{contractId}/confirm-sign")]
        public async Task<IActionResult> UpdateUserSignedStatus([FromRoute] string projectId, [FromRoute] string contractId)
        {
            try
            {
                await _contractService.UpdateSignedStatusForUserInContract(contractId, projectId);
                return Ok("Cập nhật hợp đồng thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }
        
        [HttpPost("contracts/{contractId}/download")]
        [Authorize]
        public async Task<ActionResult<DocumentDownLoadResponseDTO>> DownLoadContract([FromRoute]string projectId, [FromRoute] string contractId)
        {

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var downloadLink = await _contractService.DownLoadFileContract(userId, projectId, contractId);
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

        [HttpGet("contracts")]
        [Authorize]
        public async Task<ActionResult<SearchResponseDTO<ContractSearchResponseDTO>>> SearchContractWithFilters(
    [FromRoute] string projectId, [FromQuery] ContractSearchDTO search, int pageSize, int pageIndex)
        {
            pageIndex = pageIndex < 1 ? 1 : pageIndex;
            pageSize = pageSize < 1 ? 10 : pageSize;
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contracts = await _contractService.SearchContractWithFilters(userId, projectId, search, pageIndex, pageSize);
                return Ok(contracts);
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
    }

}
