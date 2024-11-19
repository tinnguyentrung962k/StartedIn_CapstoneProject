using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.InvestmentCall;
using StartedIn.CrossCutting.DTOs.ResponseDTO.InvestmentCall;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services;
using StartedIn.Service.Services.Interface;

namespace StartedIn.API.Controllers;

[ApiController]
[Route("api/projects/{projectId}")]
public class InvestmentCallController : ControllerBase
{
    private readonly IInvestmentCallService _investmentCallService;
    private readonly IMapper _mapper;

    public InvestmentCallController(IInvestmentCallService investmentCallService, IMapper mapper)
    {
        _investmentCallService = investmentCallService;
        _mapper = mapper;
    }

    [HttpPost("investment-call")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<InvestmentCallResponseDTO>> CreateInvestmentCall([FromRoute] string projectId,
        InvestmentCallCreateDTO investmentCallCreateDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var investmentCall =
                await _investmentCallService.CreateNewInvestmentCall(userId, projectId, investmentCallCreateDto);
            var responseInvestmentCall = _mapper.Map<InvestmentCallResponseDTO>(investmentCall);
            return CreatedAtAction(nameof(GetInvestmentCallById), new { projectId, investmentCallId = responseInvestmentCall.Id }, responseInvestmentCall);
        }
        catch (UnauthorizedProjectRoleException ex)
        {
            return StatusCode(403, ex);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpGet("investment-call/{investmentCallId}")]
    [Authorize(Roles = RoleConstants.USER)]
    public async Task<ActionResult<InvestmentCallResponseDTO>> GetInvestmentCallById([FromRoute] string projectId, [FromRoute] string investmentCallId)

    {
        try
        {
            var investmentCall = await _investmentCallService.GetInvestmentCallById(projectId, investmentCallId);
            var investmentCallResponse = _mapper.Map<InvestmentCallResponseDTO>(investmentCall);
            return Ok(investmentCallResponse);
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, MessageConstant.InternalServerError); ;
        }
    }

}