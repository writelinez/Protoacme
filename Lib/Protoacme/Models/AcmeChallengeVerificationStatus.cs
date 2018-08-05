using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Models
{
    public class AcmeChallengeVerificationStatus : AcmeChallengeStatus
    {
        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public AcmeError Error { get; set; }

        [JsonProperty("validationRecord", NullValueHandling = NullValueHandling.Ignore)]
        public List<AcmeValidationRecord> ValidationRecord { get; set; } = new List<AcmeValidationRecord>();
    }

    public class AcmeError
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }
    }

    public class AcmeValidationRecord
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("port")]
        public string Port { get; set; }

        [JsonProperty("addressUsed")]
        public string AddressUsed { get; set; }

        [JsonProperty("addressesResolved")]
        public List<string> AddressesResolved { get; set; }
    }
}
