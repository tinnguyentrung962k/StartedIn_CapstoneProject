using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Contract;

public class ContractSearchResponseDTO : IdentityResponseDTO
{
    public string ContractIdNumber { get; set; }
    public string ContractName { get; set; }
    public  ContractTypeEnum ContractType { get; set; }
    public List<UserInContractResponseDTO> Parties { get; set; }
    public DateTimeOffset LastUpdatedTime { get; set; }
    public ContractStatusEnum ContractStatus { get; set; }
}