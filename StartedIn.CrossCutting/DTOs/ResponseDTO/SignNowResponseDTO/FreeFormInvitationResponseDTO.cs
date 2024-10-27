using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO
{
    public class FreeFormInvitationResponseDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("callback_url")]
        public string CallBackUrl { get; set; }
    }
}
