using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO
{
    public class DocumentDownLoadResponseDTO
    {
        [JsonProperty("link")]
        public string DownLoadUrl { get; set; }
    }
}
