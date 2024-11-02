using StartedIn.CrossCutting.Enum;

namespace StartedIn.CrossCutting.DTOs.RequestDTO;

public class ContractSearchDTO
{
    public string? contractName { get; set; }
    public ContractTypeEnum? contractTypeEnum { get; set; }
    public List<string>? parties { get; set; }
    public DateTimeOffset? lastUpdatedStartDate { get; set; }
    public DateTimeOffset? lastUpdatedEndDate { get; set; }
    public ContractStatusEnum? contractStatusEnum { get; set; }
    
}