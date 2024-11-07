namespace StartedIn.CrossCutting.DTOs.RequestDTO.DealOffer
{
    public class DealOfferCreateDTO
    {
        public string ProjectId { get; set; }
        public decimal Amount { get; set; }
        public decimal EquityShareOffer { get; set; }
        public string TermCondition { get; set; }
    }
}
