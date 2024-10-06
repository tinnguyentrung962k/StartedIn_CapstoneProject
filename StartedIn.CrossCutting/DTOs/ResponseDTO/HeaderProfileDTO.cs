using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class HeaderProfileDTO
    {
        [JsonProperty(PropertyName = "authorities")]
        [JsonPropertyName("authorities")]
        public IEnumerable<string> UserRoles { get; set; }
        public string email { get; set; }
        public string fullName { get; set; }
        public string ProfilePicture { get; set; }
        public string bio { get; set; }
    }
}
