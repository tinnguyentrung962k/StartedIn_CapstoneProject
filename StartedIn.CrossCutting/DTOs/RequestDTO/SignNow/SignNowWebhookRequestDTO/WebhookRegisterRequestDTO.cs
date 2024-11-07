using Newtonsoft.Json;
namespace StartedIn.CrossCutting.DTOs.RequestDTO.SignNow.SignNowWebhookRequestDTO
{
    public class WebhookRegisterRequestDTO
    {
        [JsonProperty("event")]
        public string Event { get; set; }
        
        [JsonProperty("entity_id")]
        public string EntityId { get; set; }
        
        [JsonProperty("action")]
        public string Action { get; set; }
        
        [JsonProperty("attributes")]
        public WebhookAttribute WebhookAttribute { get; set; }
    }
}
