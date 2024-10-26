using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO
{
    public class SignNowDocumentResponseDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
