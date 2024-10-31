﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.SignNowWebhookRequestDTO
{
    public class WebhookAttribute
    {
        [JsonProperty("callback")]
        public string CallBack { get; set; }
        
        [JsonProperty("use_tls_12")]
        public bool UseTls12 { get; set; }

        [JsonProperty("integration_id")]
        public string IntegrationId { get; set; }

        [JsonProperty("docid_queryparam")]
        public bool DocIdQueryParam { get; set; }
        
        [JsonProperty("headers")]
        public SignNowWebhookHeader SignNowWebhookHeader { get; set; }

        [JsonProperty("include_metadata")]
        public bool IncludeMetaData { get; set; }

        [JsonProperty("secret_key")]
        public string SecretKey { get; set; }
    }
}
