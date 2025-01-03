﻿using Newtonsoft.Json;
namespace StartedIn.CrossCutting.DTOs.RequestDTO.SignNow
{
    public class SignNowFreeFormInviteCreateDTO
    {
        [JsonProperty("document_id")]
        public string DocumentId { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("cc")]
        public List<string> Cc { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("on_complete")]
        public string OnComplete { get; set; }
    }
}
