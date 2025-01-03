using StartedIn.CrossCutting.Enum;
namespace StartedIn.CrossCutting.DTOs.RequestDTO.Contract;
public class ContractSearchDTO
{
    public string? ContractIdNumber { get; set; }
    public string? ContractName { get; set; }
    public ContractTypeEnum? ContractTypeEnum { get; set; }
    public List<string>? Parties { get; set; }
    public DateTimeOffset? LastUpdatedStartDate { get; set; }
    public DateTimeOffset? LastUpdatedEndDate { get; set; }
    public ContractStatusEnum? ContractStatusEnum { get; set; }
    
}