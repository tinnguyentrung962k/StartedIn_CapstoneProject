namespace StartedIn.CrossCutting.DTOs.RequestDTO.EquityShare
{
    public class EquityShareCreateForInvestorDTO
    {
        public string UserId { get; set; }
        public int? ShareQuantity { get; set; }
        public decimal Percentage { get; set; }
        public decimal BuyPrice { get; set; }
    }
}
