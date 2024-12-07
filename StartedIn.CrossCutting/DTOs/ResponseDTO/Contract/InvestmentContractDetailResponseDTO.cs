using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Contract
{
    public class InvestmentContractDetailResponseDTO : ContractResponseDTO
    {
        public List<DisbursementInContractResponseDTO> Disbursements { get; set; }
        public string InvestorId { get; set; }
        public string InvestorName { get; set; }
        public string InvestorEmail { get; set; }
        public string InvestorPhoneNumber { get; set; }
        public string DealOfferId { get; set; }
        public decimal SharePercentage { get; set; }
        public decimal BuyPrice { get; set; }
        public string ProjectName { get; set; }
    }
}
