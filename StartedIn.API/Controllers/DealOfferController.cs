﻿using AutoMapper;
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
using StartedIn.Domain.Entities;
using StartedIn.CrossCutting.DTOs.ResponseDTO;

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
                _logger.LogError(ex, "Error while creating a deal");
                return StatusCode(500, MessageConstant.InternalServerError);

            }
        }

        [HttpGet("deal-offers")]
        [Authorize(Roles = RoleConstants.INVESTOR)]
        public async Task<ActionResult<PaginationDTO<DealOfferForInvestorResponseDTO>>> GetDealListOfAnInvestor([FromQuery] int page, int size)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var dealList = await _dealOfferService.GetDealOfferForAnInvestor(userId, page, size);
                return Ok(dealList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpDelete("deal-offers/{dealOfferId}")]
        [Authorize(Roles = RoleConstants.INVESTOR)]
        public async Task<IActionResult> DeleteADealOffer([FromRoute] string dealOfferId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await _dealOfferService.DeleteADealOffer(userId, dealOfferId);
                return Ok("Xoá yêu cầu thành công");
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UpdateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageConstant.InternalServerError);
            }
        }

        [HttpGet("projects/{projectId}/deal-offers")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<PaginationDTO<DealOfferForProjectResponseDTO>>> GetDealListForAProject([FromRoute] string projectId, [FromQuery] int page, int size)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var dealList = await _dealOfferService.GetDealOfferForAProject(userId, projectId, page, size);
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
                var response = await _dealOfferService.GetById(dealId);
                return Ok(response);
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
            catch (InvalidInputException ex)
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
        [HttpGet("deal-offers/{dealId}")]
        [Authorize(Roles = RoleConstants.INVESTOR)]
        public async Task<ActionResult<DealOfferForInvestorResponseDTO>> GetDealOfferDetailForInvestor([FromRoute] string dealId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var deal = await _dealOfferService.GetDealOfferForInvestorById(userId,dealId);
                return Ok(deal);
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
