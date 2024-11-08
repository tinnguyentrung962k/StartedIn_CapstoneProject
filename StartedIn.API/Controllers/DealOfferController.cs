using AutoMapper;
using CrossCutting.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.Contract;
using StartedIn.CrossCutting.DTOs.RequestDTO.DealOffer;
using StartedIn.CrossCutting.DTOs.ResponseDTO.DealOffer;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;
using StartedIn.API.Attributes;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class DealOfferController : ControllerBase
    {
        private readonly IDealOfferService _dealOfferService;
        private readonly IMapper _mapper;
        private readonly ILogger<DealOfferController> _logger;

        public DealOfferController(IDealOfferService dealOfferService, IMapper mapper, ILogger<DealOfferController> logger)
        {
            _dealOfferService = dealOfferService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("deal-offers")]
        [Authorize(Roles = RoleConstants.INVESTOR)]
        public async Task<ActionResult<DealOfferForProjectResponseDTO>> SendADealOffer(DealOfferCreateDTO dealOfferCreateDTO)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var dealOfferEntity = await _dealOfferService.SendADealOffer(userId, dealOfferCreateDTO);
                var dealOfferResponse = _mapper.Map<DealOfferForProjectResponseDTO>(dealOfferEntity);
                return Ok(dealOfferResponse);
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating a deal");
                return StatusCode(500, MessageConstant.InternalServerError);

            }
        }

        [HttpGet("deal-offers")]
        [Authorize(Roles = RoleConstants.INVESTOR)]
        public async Task<ActionResult<SearchResponseDTO<DealOfferForInvestorResponseDTO>>> GetDealListOfAnInvestor([FromQuery] int pageIndex, int pageSize)
        {
            pageIndex = pageIndex < 1 ? 1 : pageIndex;
            pageSize = pageSize < 1 ? 10 : pageSize;
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var dealList = await _dealOfferService.GetDealOfferForAnInvestor(userId, pageIndex, pageSize);
                return Ok(dealList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpGet("projects/{projectId}/deal-offers")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<SearchResponseDTO<DealOfferForProjectResponseDTO>>> GetDealListForAProject([FromRoute] string projectId, [FromQuery] int pageIndex, int pageSize)
        {
            pageIndex = pageIndex < 1 ? 1 : pageIndex;
            pageSize = pageSize < 1 ? 10 : pageSize;
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var dealList = await _dealOfferService.GetDealOfferForAProject(userId, projectId, pageIndex, pageSize);
                return Ok(dealList);
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

        [HttpGet("projects/{projectId}/deal-offers/{dealId}")]
        [Authorize(Roles = RoleConstants.USER), RequireProjectAccess]
        public async Task<ActionResult<DealOfferForProjectResponseDTO>> GetByIdInProject([FromRoute] string dealId)
        {
            try
            {
                return Ok(_mapper.Map<DealOfferForProjectResponseDTO>(await _dealOfferService.GetById(dealId)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpPut("projects/{projectId}/deal-offers/{dealId}/accept")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<DealOfferForProjectResponseDTO>> AcceptDeal([FromRoute] string projectId, [FromRoute] string dealId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var deal = await _dealOfferService.AcceptADeal(userId, projectId, dealId);
                var response = _mapper.Map<DealOfferForProjectResponseDTO>(deal);
                return Ok(response);
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
                return StatusCode(500, MessageConstant.InternalServerError);
            }

        }

        [HttpPut("projects/{projectId}/deal-offers/{dealId}/reject")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<DealOfferForProjectResponseDTO>> RejectDeal([FromRoute] string projectId, [FromRoute] string dealId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var deal = await _dealOfferService.RejectADeal(userId, projectId, dealId);
                var response = _mapper.Map<DealOfferForProjectResponseDTO>(deal);
                return Ok(response);
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
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }
    }
}
