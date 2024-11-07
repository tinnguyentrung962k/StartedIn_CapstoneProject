namespace StartedIn.CrossCutting.DTOs.RequestDTO.EquityShare
{
    public class EquityShareCreateForMemberDTO
    {
        public string UserId { get; set; }
        public int? ShareQuantity { get; set; }
        public decimal? Percentage { get; set; }
    }
}
