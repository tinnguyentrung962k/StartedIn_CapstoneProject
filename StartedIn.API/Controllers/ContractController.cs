using AutoMapper;
using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Contract;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Contract;
using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.DealOffer;
using StartedIn.Domain.Entities;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}")]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IMapper _mapper;
        private readonly ILogger<ContractController> _logger;
        public ContractController(IContractService contractService, IMapper mapper, ILogger<ContractController> logger)
        {
            _contractService = contractService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("investment-contracts")]
        [Authorize(Roles = RoleConstants.USER)]
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
            catch (InvalidInputException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating contract");

                return StatusCode(500, MessageConstant.InternalServerError);

            }
        }

        [HttpPost("contracts/{contractId}/liquidation-notes")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<ContractResponseDTO>> CreateLiquidationNote([FromRoute] string projectId, [FromRoute] string contractId, IFormFile uploadFile)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.CreateLiquidationNote(userId,projectId,contractId,uploadFile);
                var responseContract = _mapper.Map<ContractResponseDTO>(contract);
                return CreatedAtAction(nameof(GetContractById), new { projectId, contractId = responseContract.Id }, responseContract);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpPost("contracts/{contractId}/terminated-meeting")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<IActionResult> CreateMeetingToTerminateContractByLeader([FromRoute] string projectId, [FromRoute] string contractId, TerminationMeetingCreateDTO terminationMeetingCreateDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _contractService.CreateMeetingForLeaderTerminationContract(userId, projectId, contractId, terminationMeetingCreateDTO);
                return StatusCode(201, "Tạo cuộc họp thành công");
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UpdateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpPut("contracts/{contractId}/liquidation-cancel")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<IActionResult> CancelLiquidationOfAContract([FromRoute] string projectId, [FromRoute] string contractId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _contractService.CancelLiquidationAfterMeeting(userId,projectId,contractId);
                return Ok("Huỷ thanh lý thành công");
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnmatchedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpPost("investment-contracts/from-deal")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<ContractResponseDTO>> CreateInvestmentContractFromDeal([FromRoute] string projectId, [FromBody] InvestmentContractFromDealCreateDTO investmentContractFromDealCreateDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.CreateInvestmentContractFromDeal(userId, projectId, investmentContractFromDealCreateDTO);
                var response = _mapper.Map<ContractResponseDTO>(contract);
                return Ok(response);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (ExistedRecordException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidInputException ex)
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

        [HttpPost("shares-distribution-contracts")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<ContractResponseDTO>> CreateStartupShareAllMemberContract([FromRoute] string projectId, [FromBody] GroupContractCreateDTO groupContractCreateDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.CreateStartupShareAllMemberContract(userId, projectId, groupContractCreateDTO);
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
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidInputException ex)
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
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<ContractResponseDTO>> UpdateInvestmentContract([FromRoute] string projectId, [FromRoute] string contractId, [FromBody] InvestmentContractUpdateDTO investmentContractUpdateDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.UpdateInvestmentContract(userId, projectId, contractId, investmentContractUpdateDTO);
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
        [HttpPut("shares-distribution-contracts/{contractId}")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<ContractResponseDTO>> UpdateStartupShareAllMemberContract([FromRoute] string projectId, [FromRoute] string contractId, [FromBody] GroupContractUpdateDTO groupContractUpdateDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.UpdateStartupShareAllMemberContract(userId, projectId, contractId, groupContractUpdateDTO);
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
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR)]
        public async Task<ActionResult<InvestmentContractDetailResponseDTO>> GetInvestmentContractDetail([FromRoute] string projectId, [FromRoute] string contractId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.GetContractByContractId(userId, contractId, projectId);
                var responseContract = _mapper.Map<InvestmentContractDetailResponseDTO>(contract);
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
        [HttpGet("liquidation-notes/{contractId}")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.MENTOR + "," + RoleConstants.INVESTOR)]
        public async Task<ActionResult<GroupContractDetailResponseDTO>> GetLiquidationNoteDetail([FromRoute] string projectId, [FromRoute] string contractId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.GetContractByContractId(userId, contractId, projectId);
                var responseContract = _mapper.Map<LiquidationNoteDetailResponseDTO>(contract);
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

        [HttpGet("shares-distribution-contracts/{contractId}")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<GroupContractDetailResponseDTO>> GetStartupShareAllMemberContractDetail([FromRoute] string projectId, [FromRoute] string contractId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.GetContractByContractId(userId, contractId, projectId);
                var responseContract = _mapper.Map<GroupContractDetailResponseDTO>(contract);
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
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<ContractResponseDTO>> SendInviteForContract([FromRoute] string projectId, [FromRoute] string contractId)
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
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.INVESTOR)]
        public async Task<ActionResult<ContractResponseDTO>> GetContractById([FromRoute] string projectId, [FromRoute] string contractId)

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
        public async Task<IActionResult> ValidAcontract([FromRoute] string projectId, [FromRoute] string contractId)
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
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<DocumentDownLoadResponseDTO>> DownLoadContract([FromRoute] string projectId, [FromRoute] string contractId)

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
        [HttpPut("contracts/{contractId}/cancel")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<ContractResponseDTO>> CancelAContract([FromRoute] string projectId, [FromRoute] string contractId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contract = await _contractService.CancelContract(userId, projectId, contractId);
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

        [HttpGet("contracts")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<PaginationDTO<ContractSearchResponseDTO>>> SearchContractWithFilters(
    [FromRoute] string projectId, [FromQuery] ContractSearchDTO search, int page, int size)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contracts = await _contractService.SearchContractWithFilters(userId, projectId, search, page, size);
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
        [HttpPost("contracts/{contractId}/expiration")]
        public async Task<IActionResult> MarkContractAsExpired([FromRoute] string projectId, [FromRoute] string contractId)
        {
            try
            {
                await _contractService.MarkExpiredContract(projectId, contractId);
                return Ok("Cập nhật hợp đồng thành công");
            }
            catch (UpdateException ex)
            {
                return BadRequest(ex.Message);
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
                return StatusCode(500, ex.Message);
            }

        }

        [HttpDelete("contracts/{contractId}")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<IActionResult> DeleteDraftContract([FromRoute] string projectId, [FromRoute] string contractId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _contractService.DeleteContract(userId, projectId, contractId);
                return Ok("Xoá hợp đồng thành công");
            }
            catch (UnauthorizedProjectRoleException ex)
            {
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
                return StatusCode(500, ex.Message);
            }

        }

        [HttpGet("contracts/{contractId}/history")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR + "," + RoleConstants.MENTOR)]
        public async Task<ActionResult<List<UserInContractHistoryResponseDTO>>> GetContractSignedHistory([FromRoute] string projectId, [FromRoute] string contractId)
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var contractSignHistory = await _contractService.GetUserSignHistoryInAContract(userId, projectId, contractId);
                var response = _mapper.Map<List<UserInContractHistoryResponseDTO>>(contractSignHistory);
                return Ok(response);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
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
                return StatusCode(500, ex.Message);
            }
        }
    }

}
