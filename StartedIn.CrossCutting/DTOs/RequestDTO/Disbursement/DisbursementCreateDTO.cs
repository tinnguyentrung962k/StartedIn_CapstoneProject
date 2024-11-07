namespace StartedIn.CrossCutting.DTOs.RequestDTO.Disbursement
{
    public class DisbursementCreateDTO
    {
        public string Title { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal Amount { get; set; }
        public string Condition { get; set; }
    }
}
