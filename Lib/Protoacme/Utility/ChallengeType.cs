using Protoacme.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Utility
{
    public class ChallengeType
    {
        private string _value = string.Empty;

        public string Value { get { return _value; } }

        private ChallengeType(string value)
        {
            _value = value;
        }

        public static ChallengeType Http
        {
            get
            {
                return new ChallengeType(ProtoacmeContants.CHALLENGE_HTTP);
            }
        }

        public static ChallengeType Dns
        {
            get
            {
                return new ChallengeType(ProtoacmeContants.CHALLENGE_DNS);
            }
        }

        public static ChallengeType Tls
        {
            get
            {
                return new ChallengeType(ProtoacmeContants.CHALLENGE_TLS);
            }
        }
    }
}
