namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Payment
{
    public class PaymentResultResponseDTO
    {
        public string Code { get; set; }
        public string Desc { get; set; }
        public PayOsCreatePaymentResult CreatedPaymentResult { get; set; }
        public string? Signature { get; set; }
    }
}
