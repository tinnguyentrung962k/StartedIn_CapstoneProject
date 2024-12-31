using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
using StartedIn.CrossCutting.Enum;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.DealOffer
{
    public class DealOfferForInvestorResponseDTO : IdentityResponseDTO
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string LeaderId { get; set; }
        public string LeaderName { get; set; }
        public string Amount { get; set; }
        public string EquityShareOffer { get; set; }
        public string TermCondition { get; set; }
        public DealStatusEnum DealStatus { get; set; }
        public List<DisbursementInDealOfferDTO>? Disbursements { get; set; }
    }
}
