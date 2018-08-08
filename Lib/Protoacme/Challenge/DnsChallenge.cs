using Protoacme.Models;
using Protoacme.Utility.Certificates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Challenge
{
    public class DnsChallenge : IAcmeChallengeContent
    {
        public AcmeAccount Account { get; set; }

        public AcmeChallenge Challenge { get; set; }

        public string AuthorizationKey { get; set; }

        public string Token { get; set; }

        public DnsChallenge(AcmeAccount account, AcmeChallenge challenge)
        {
            Account = account;
            Challenge = challenge;

            Token = challenge.Token;
            AuthorizationKey = CertificateUtility.CreateAuthorizationKey(account, challenge.Token);
        }

        public void SaveToFile(string filePath)
        {
            throw new NotImplementedException("Not implemented on Dns challenge. Requires DNS TXT record change with authorization key.");
        }
    }
}
