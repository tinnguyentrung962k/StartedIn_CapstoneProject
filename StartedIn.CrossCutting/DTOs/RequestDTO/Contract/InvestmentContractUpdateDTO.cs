using StartedIn.CrossCutting.DTOs.RequestDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.RequestDTO.EquityShare;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Contract
{
    public class InvestmentContractUpdateDTO
    {
        public ContractUpdateDTO Contract { get; set; }
        public EquityShareUpdateForInvestorDTO InvestorInfo { get; set; }
        public List<DisbursementUpdateDTO>? Disbursements { get; set; }
    }
}
