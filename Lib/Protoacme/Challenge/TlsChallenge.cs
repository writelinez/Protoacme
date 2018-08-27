using Protoacme.Models;
using Protoacme.Utility.Certificates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Protoacme.Challenge
{
    public class TlsChallenge : IAcmeChallengeContent
    {
        public AcmeAccount Account { get; set; }

        public AcmeChallenge Challenge { get; set; }

        public string AuthorizationKey { get; set; }

        public string Token { get; set; }

        public string Identifier { get; set; }

        public TlsChallenge(AcmeAccount account, AcmeChallenge challenge, string identifier)
        {
            Account = account;
            Challenge = challenge;
            Identifier = identifier;

            Token = challenge.Token;
            AuthorizationKey = CertificateUtility.CreateAuthorizationKey(account, challenge.Token);
        }

        public TlsChallenge() { }

        public void SaveToFile(string filePath)
        {
            throw new NotSupportedException("Currently not supported.");
        }
    }
}
