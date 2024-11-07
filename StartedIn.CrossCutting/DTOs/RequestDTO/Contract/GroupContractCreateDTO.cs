using StartedIn.CrossCutting.DTOs.RequestDTO.EquityShare;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Contract
{
    public class GroupContractCreateDTO
    {
        public ContractCreateDTO Contract { get; set; }
        public List<EquityShareCreateForMemberDTO> ShareEquitiesOfMembers { get; set; }
    }
}
