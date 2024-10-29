using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
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

        [HttpPost("/contract")]
        [Authorize]
        public async Task<ActionResult<ContractResponseDTO>> CreateAContract([FromBody]ContractCreateThreeModelsDTO contractCreateThreeModelsDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            try
            {
                var contract = await _contractService.CreateAContract(userId, contractCreateThreeModelsDTO);
                var responseContract = _mapper.Map<ContractResponseDTO>(contract);

                return Ok(responseContract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating contract");
                return StatusCode(500, "Lỗi server");
            }
        }
        [HttpPost("/contract/upload-contract/{contractId}")]
        [Authorize]
        public async Task<ActionResult<ContractResponseDTO>> UploadContractFile([FromRoute]string contractId,IFormFile uploadFile)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            try
            {
                var contract = await _contractService.UploadContractFile(userId,contractId,uploadFile);
                var responseContract = _mapper.Map<ContractResponseDTO>(contract);

                return Ok(responseContract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating contract");
                return StatusCode(500, "Lỗi server");
            }
        }


    }
}
