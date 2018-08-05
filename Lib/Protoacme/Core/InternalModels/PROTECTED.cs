using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Core.InternalModels
{
    internal class PROTECTED
    {
        public string alg { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JWK jwk { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string kid { get; set; }

        public string nonce { get; set; }

        public string url { get; set; }
    }
}
