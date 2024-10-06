using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class FullProfileDTO : IdentityResponseDTO
    {
        [JsonProperty(PropertyName = "authorities")]
        [JsonPropertyName("authorities")]
        public IEnumerable<string> UserRoles { get; set; }
        public string ProfilePicture { get; set; }
        public string CoverPhoto { get; set; }
        public string Bio { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}
