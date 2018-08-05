using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Models
{
    public class AcmeAuthorization
    {
        public string Status { get; set; }

        public DateTime Expires { get; set; }

        public AcmeIdentifier Identifier { get; set; }

        public List<AcmeChallenge> Challenges { get; set; } = new List<AcmeChallenge>();
    }
}
