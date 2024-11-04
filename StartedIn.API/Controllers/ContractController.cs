using AutoMapper;
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
        
        [HttpPost("investment-contract")]
        [Authorize]
        public async Task<ActionResult<ContractResponseDTO>> CreateAnInvestmentContract([FromBody] InvestmentContractCreateDTO investmentContractCreateDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.CreateInvestmentContract(userId, investmentContractCreateDTO);
                var responseContract = _mapper.Map<ContractResponseDTO>(contract);
                return CreatedAtAction(nameof(GetContractById), new { contractId = responseContract.Id }, responseContract);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403,ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating contract");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpPost("contracts/send-invite/{contractId}")]
        [Authorize]
        public async Task<ActionResult<ContractResponseDTO>> SendInviteForContract([FromRoute] string contractId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.SendSigningInvitationForContract(userId, contractId);
                var responseContract = _mapper.Map<ContractResponseDTO>(contract);

                return Ok(responseContract);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating contract");
                return StatusCode(500, "Lỗi server");
            }
        }
        
        [HttpGet("contracts/user-contract/project/{projectId}")]
        [Authorize]
        public async Task<ActionResult<List<ContractResponseDTO>>> GetPersonalContractsInAProject([FromRoute] string projectId, [FromQuery] int pageIndex, int pageSize)
        {
            
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contracts = await _contractService.GetContractsByUserIdInAProject(userId, projectId, pageIndex, pageSize);
                var responseContract = _mapper.Map<List<ContractResponseDTO>>(contracts);
                return Ok(responseContract);
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
                _logger.LogError(ex, "Error while creating contract");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpGet("contracts/{contractId}")]
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
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi truy xuất"); ;
            }
        }

        [HttpPost("contracts/valid-contract/{contractId}")]
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

        [HttpPost("contracts/update-user-sign/{contractId}")]
        public async Task<IActionResult> UpdateUserSignedStatus([FromRoute] string contractId)
        {
            try
            {
                await _contractService.UpdateSignedStatusForUserInContract(contractId);
                return Ok("Cập nhật hợp đồng thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi Cập nhật");
            }
        }
        
        [HttpPost("contracts/download-contract/{contractId}")]
        [Authorize]
        public async Task<ActionResult<DocumentDownLoadResponseDTO>> DownLoadContract([FromRoute] string contractId)
        {
            
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var downloadLink = await _contractService.DownLoadFileContract(userId,contractId);
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
                return StatusCode(500, "Lỗi tải tập tin");
            }
        }

        [HttpGet("contracts/project-contracts/{projectId}/search")]
        [Authorize]
        public async Task<ActionResult<SearchResponseDTO<ContractSearchResponseDTO>>> SearchContractWithFilters(
    [FromRoute] string projectId, [FromQuery] ContractSearchDTO search, int pageSize, int pageIndex)
        {

            try
            {
                pageIndex = pageIndex < 1 ? 1 : pageIndex;
                pageSize = pageSize < 1 ? 10 : pageSize;

                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                // Call the service to perform the search
                var contracts = await _contractService.SearchContractWithFilters(userId, projectId, search, pageIndex, pageSize);

                // Prepare the response with pagination and mapping to DTO
                var contractResponseList = contracts.Select(c => _mapper.Map<ContractSearchResponseDTO>(c)).ToList();

                var totalRecords = contracts.Count(); // Assuming a count method exists
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var response = new SearchResponseDTO<ContractSearchResponseDTO>
                {
                    ResponseList = contractResponseList,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalRecord = totalRecords,
                    TotalPage = totalPages
                };

                return Ok(response);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
        
}
