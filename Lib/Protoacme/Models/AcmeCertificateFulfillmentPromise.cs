using Newtonsoft.Json;
using Protoacme.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Models
{
    public class AcmeCertificateFulfillmentPromise
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("expires")]
        [JsonConverter(typeof(ISO8601DateConverter))]
        public DateTime Expires { get; set; }

        //NOT SUPPORTED
        //[JsonProperty(
        //    PropertyName = "notBefore",
        //    NullValueHandling = NullValueHandling.Ignore)]
        //[JsonConverter(typeof(ISO8601DateConverter))]
        //public DateTime NotBefore { get; set; }

        //NOT SUPPORTED
        //[JsonProperty(
        //    PropertyName = "notAfter",
        //    NullValueHandling = NullValueHandling.Ignore)]
        //[JsonConverter(typeof(ISO8601DateConverter))]
        //public DateTime NotAfter { get; set; }

        [JsonProperty("identifiers")]
        public List<AcmeIdentifier> Identifiers { get; set; }

        [JsonProperty("authorizations")]
        public List<string> Authorizations { get; set; }

        [JsonProperty("finalize")]
        public string Finalize { get; set; }

        [JsonProperty("certificate", NullValueHandling = NullValueHandling.Ignore)]
        public string Certificate { get; set; }
    }

    public class AcmeIdentifier
    {
        [JsonProperty("type")]
        public virtual string Type { get; set; }

        [JsonProperty("value")]
        public virtual string Value { get; set; }
    }
}
