using Newtonsoft.Json;
using Protoacme.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Models
{
    public class AcmeChallengeStatus : AcmeChallenge
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty(
            PropertyName = "validated",
            NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ISO8601DateConverter))]
        public DateTime? Validated { get; set; }

        [JsonProperty("keyAuthorization", NullValueHandling = NullValueHandling.Ignore)]
        public string KeyAuthorization { get; set; }
    }
}
