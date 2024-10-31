using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.SignNowWebhookRequestDTO
{
    public class SignNowWebhookHeader
    {
        [JsonProperty("string_head")]
        public string StringHead { get; set; }
       
        [JsonProperty("int_head")]
        public int IntHead { get; set; }
        
        [JsonProperty("bool_head")]
        public bool BoolHead { get; set; }
        
        [JsonProperty("float_head")]
        public float FloatHead { get; set; }
    }
}
