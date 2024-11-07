using Newtonsoft.Json;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO
{
    public class DocumentDownLoadResponseDTO
    {
        [JsonProperty("link")]
        public string DownLoadUrl { get; set; }
    }
}
