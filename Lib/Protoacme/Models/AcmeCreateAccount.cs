using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Models
{
    public class AcmeCreateAccount
    {
        [JsonProperty("termsOfServiceAgreed")]
        public bool TermsOfServiceAgreed { get; set; }

        [JsonProperty("contact")]
        public List<string> Contact { get; set; } = new List<string>();
    }
}
