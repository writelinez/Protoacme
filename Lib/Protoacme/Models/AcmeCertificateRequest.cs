using Newtonsoft.Json;
using Protoacme.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Models
{
    public class AcmeCertificateRequest
    {
        [JsonProperty("identifiers")]
        public List<DnsCertificateIdentifier> Identifiers { get; set; } = new List<DnsCertificateIdentifier>();

        //NOT SUPPORTED BY LETSENCRYPT
        //[JsonProperty(
        //    PropertyName = "notBefore", 
        //    NullValueHandling = NullValueHandling.Ignore)]
        //[JsonConverter(typeof(ISO8601DateConverter))]
        //public DateTime? NotBefore { get; set; }

        //NOT SUPPORTED BY LETSENCRYPT
        //[JsonProperty(
        //    PropertyName = "notAfter",
        //    NullValueHandling = NullValueHandling.Ignore)]
        //[JsonConverter(typeof(ISO8601DateConverter))]
        //public DateTime? NotAfter { get; set; }
    }

    public abstract class CertificateIdentifier
    {
        [JsonProperty("type")]
        public virtual string Type { get; private set; }

        [JsonProperty("value")]
        public virtual string Value { get; set; }
    }

    public class DnsCertificateIdentifier : CertificateIdentifier
    {
        private string _type = "dns";

        public override string Type => _type;
    }
}
