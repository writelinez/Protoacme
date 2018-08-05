using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Models
{
    public class AcmeChallenge
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
