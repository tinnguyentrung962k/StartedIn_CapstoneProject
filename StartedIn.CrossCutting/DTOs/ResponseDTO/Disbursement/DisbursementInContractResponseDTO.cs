using StartedIn.CrossCutting.DTOs.BaseDTO;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement
{
    public class DisbursementInContractResponseDTO : IdentityResponseDTO
    {
        public string Title { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal Amount { get; set; }
        public string Condition { get; set; }
    }
}
