using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Models
{
    public class AcmeDirectory
    {
        [JsonProperty("keyChange")]
        public string KeyChange { get; set; }

        [JsonProperty("meta")]
        public AcmeMeta Meta { get; set; }

        [JsonProperty("newAccount")]
        public string NewAccount { get; set; }

        [JsonProperty("newNonce")]
        public string NewNoonce { get; set; }

        [JsonProperty("newOrder")]
        public string NewOrder { get; set; }

        [JsonProperty("revokeCert")]
        public string RevokeCert { get; set; }

        [JsonProperty("vPSDoAUTX1E")]
        public string VPSDoAUTX1E { get; set; }
    }

    public class AcmeMeta
    {
        [JsonProperty("caaIdentities")]
        public List<string> CaaIdentities { get; set; } = new List<string>();

        [JsonProperty("termsOfService")]
        public string Tos { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }
    }
}
