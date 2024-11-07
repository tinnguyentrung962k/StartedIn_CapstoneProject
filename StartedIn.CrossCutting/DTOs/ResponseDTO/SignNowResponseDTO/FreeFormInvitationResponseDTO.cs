using Newtonsoft.Json;

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
