using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
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
    public DateOnly? ValidDate { get; set; }
    public decimal? TotalDisbursementAmount { get; set; }
    public decimal? DisbursedAmount { get; set; }
    public decimal? PendingAmount { get; set; }
    public string? LiquidationNoteId { get; set; }
    public string? ParentContractId { get; set; }
    public List<DisbursementInContractListResponseDTO> Disbursements { get; set; }
}