using StartedIn.CrossCutting.DTOs.RequestDTO.Disbursement;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.DealOffer
{
    public class DealOfferCreateDTO
    {
        public string ProjectId { get; set; }
        public decimal Amount { get; set; }
        public decimal EquityShareOffer { get; set; }
        public string TermCondition { get; set; }
        public List<DisbursementCreateDTO>? Disbursements { get; set; }
    }
}
