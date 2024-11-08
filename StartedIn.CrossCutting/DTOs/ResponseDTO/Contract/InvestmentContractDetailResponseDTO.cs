using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Contract
{
    public class InvestmentContractDetailResponseDTO : ContractResponseDTO
    {
        public List<DisbursementInContractResponseDTO> Disbursements { get; set; }
        public string DealOfferId { get; set; }
    }
}
