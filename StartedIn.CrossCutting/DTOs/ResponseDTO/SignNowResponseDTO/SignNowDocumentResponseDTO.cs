using Newtonsoft.Json;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO
{
    public class SignNowDocumentResponseDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
