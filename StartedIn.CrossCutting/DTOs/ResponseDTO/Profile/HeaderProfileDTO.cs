﻿using Newtonsoft.Json;
using StartedIn.CrossCutting.DTOs.BaseDTO;
using System.Text.Json.Serialization;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class HeaderProfileDTO : IdentityResponseDTO
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
