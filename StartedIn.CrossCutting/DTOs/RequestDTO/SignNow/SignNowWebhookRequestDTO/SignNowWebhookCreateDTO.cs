namespace StartedIn.CrossCutting.DTOs.RequestDTO.SignNow.SignNowWebhookRequestDTO
{
    public class SignNowWebhookCreateDTO
    {
        public string EntityId { get; set; }
        public string Event { get; set; }
        public string Action { get; set; }
        public string CallBackUrl { get; set; }

    }
}
