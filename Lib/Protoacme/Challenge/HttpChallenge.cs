﻿using Protoacme.Models;
using Protoacme.Utility.Certificates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Protoacme.Challenge
{
    public class HttpChallenge : IAcmeChallengeContent
    {
        public AcmeAccount Account { get; set; }

        public AcmeChallenge Challenge { get; set; }

        public string AuthorizationKey { get; set; }

        public string Token { get; set; }

        public string Identifier { get; set; }

        public HttpChallenge(AcmeAccount account, AcmeChallenge challenge, string identifier)
        {
            Account = account;
            Challenge = challenge;
            Identifier = identifier;

            Token = challenge.Token;
            AuthorizationKey = CertificateUtility.CreateAuthorizationKey(account, challenge.Token);
        }

        public HttpChallenge() { }

        public void SaveToFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            if (string.IsNullOrEmpty(fileName))
                throw new FormatException("filePath missing filename.");

            string directory = filePath.Replace(fileName, "");
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException(directory);

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(AuthorizationKey);
                fs.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
