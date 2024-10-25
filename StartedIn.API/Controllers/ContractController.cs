using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SignNow.Net.Model;
using StartedIn.CrossCutting.Customize;
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
        public ContractController(IContractService contractService, IMapper mapper, ILogger<ContractController> logger)
        {
            _contractService = contractService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("/contract")]
        [Authorize]
        public async Task<ActionResult<ContractResponseDTO>> CreateAContract([FromForm] ContractCreateDTO contractCreateDTO,[FromForm] string editableFieldsJson)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            try
            {
                // Manually deserialize the JSON string to a list of EditableField objects
                var editableFields = JsonConvert.DeserializeObject<List<EditableField>>(editableFieldsJson);

                if (editableFields == null || !editableFields.Any())
                {
                    throw new Exception("Editable fields are required and must not be empty.");
                }

                // Call the service to create the contract
                var contract = await _contractService.CreateAContract(userId, contractCreateDTO, editableFields);
                var responseContract = _mapper.Map<ContractResponseDTO>(contract);

                return Ok(responseContract);
            }
            catch (JsonSerializationException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON serialization error while creating contract");
                return BadRequest("Invalid JSON format for editable fields.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating contract");
                return StatusCode(500, "Lỗi server");
            }
        }
    }
}
