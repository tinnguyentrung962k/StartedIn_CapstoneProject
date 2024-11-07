using StartedIn.CrossCutting.DTOs.RequestDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.RequestDTO.EquityShare;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Contract
{
    public class InvestmentContractCreateDTO
    {
        public ContractCreateDTO Contract { get; set; }
        public EquityShareCreateForInvestorDTO InvestorInfo { get; set; }
        public List<DisbursementCreateDTO>? Disbursements { get; set; }
    }
}
