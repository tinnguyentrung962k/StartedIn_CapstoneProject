using Microsoft.AspNetCore.Http;
namespace StartedIn.CrossCutting.DTOs.RequestDTO.Contract
{
    public class ContractUploadFileDTO
    {
        public string ContractId { get; set; }
        public IFormFile ContractFile { get; set; }
    }
}
