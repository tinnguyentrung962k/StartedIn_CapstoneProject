using StartedIn.CrossCutting.DTOs.RequestDTO.Disbursement;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Contract
{
    public class InvestmentContractFromDealCreateDTO
    {
        public ContractCreateDTO Contract { get; set; }
        public List<DisbursementCreateDTO>? Disbursements { get; set; }
    }
}
