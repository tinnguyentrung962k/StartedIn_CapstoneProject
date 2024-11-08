using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Contract
{
    public class InvestmentContractDetailResponseDTO : ContractResponseDTO
    {
        public List<DisbursementInContractResponseDTO> Disbursements { get; set; }
        public UserInContractResponseDTO InvestorInfo { get; set; }
        public string DealOfferId { get; set; }
        public decimal SharePercentage { get; set; }
        public decimal BuyPrice { get; set; }
    }
}
