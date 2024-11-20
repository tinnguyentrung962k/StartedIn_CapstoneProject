using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Transaction;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Transaction;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Service.Services.Interface;
using System.Security.Claims;

namespace StartedIn.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger, IMapper mapper, IUserService userService)
        {
             _logger = logger;
            _transactionService = transactionService;
            _mapper = mapper;
            _userService = userService;
        }

        [HttpGet("transactions")]
        [Authorize(Roles = RoleConstants.INVESTOR +","+ RoleConstants.USER)]
        public async Task<ActionResult<PaginationDTO<TransactionResponseDTO>>> GetTransactionListInAProject([FromRoute] string projectId, [FromQuery] int page, int size)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var transactionList = await _transactionService.GetListTransactionOfAProject(userId, projectId, page, size);
                return Ok(transactionList);
            }
            catch (UnauthorizedProjectRoleException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpPost("transactions")]
        [Authorize(Roles = RoleConstants.USER)]
        public async Task<ActionResult<TransactionResponseDTO>> AddNewTransaction([FromRoute] string projectId, [FromForm] TransactionCreateDTO transactionCreateDTO )
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var transaction = await _transactionService.AddAnTransactionForProject(userId, projectId, transactionCreateDTO);
                var response = _mapper.Map<TransactionResponseDTO>(transaction);
                return CreatedAtAction(nameof(GetTransactionById), new { projectId, transactionId = response.Id }, response);
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
                return StatusCode(500, ex.Message);
            }

        }

        [HttpGet("transactions/{transactionId}")]
        [Authorize(Roles = RoleConstants.USER + "," + RoleConstants.INVESTOR)]
        public async Task<ActionResult<TransactionResponseDTO>> GetTransactionById([FromRoute] string projectId, [FromRoute] string transactionId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var transaction = await _transactionService.GetTransactionDetailById(userId, projectId, transactionId);
                var response = _mapper.Map<TransactionResponseDTO>(transaction);
                return Ok(response);
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
                return StatusCode(500, ex.Message);
            }
        }


    }
}
