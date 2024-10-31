using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.SignNowWebhookRequestDTO
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
