using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Transaction;
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
                return transactionList;
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        
    }
}
