using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Core
{
    public class ProtoacmeContants
    {
        public const string LETSENCRYPT_PROD_ENDPOINT = "https://acme-staging-v02.api.letsencrypt.org/";
        public const string LETSENCRYPT_STAGING_ENDPOINT = "https://acme-staging-v02.api.letsencrypt.org/";

        public const string LETSENCRYPT_DIRECTORY_FRAGMENT = "directory";
        public const string LETSENCRYPT_ACCOUNT_FRAGMENT = "acme/acct/";

        public const string HEADER_NONCE = "Replay-Nonce";
        public const string HEADER_LOCATION = "Location";

        public const string CHALLENGE_HTTP = "http-01";
        public const string CHALLENGE_DNS = "dns-01";
        public const string CHALLENGE_TLS = "tls-alpn-01";
    }
}
