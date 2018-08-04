using Newtonsoft.Json;
using Protoacme.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Protoacme.Core
{
    public class JwsContainer<TPayload>
        where TPayload : class
    {
        private readonly RSAParameters _rsaParameters;
        private readonly RSACryptoServiceProvider _cryptoProvider;
        private readonly string _nonce;
        private readonly string _directory;
        private readonly string _kid;
        
        public TPayload Payload { get; set; }

        public JwsContainer(RSAParameters rsaParameters, string nonce, string directory, string kid, TPayload payload = null)
        {
            _cryptoProvider = new RSACryptoServiceProvider(2048);
            _rsaParameters = rsaParameters;
            _cryptoProvider.ImportParameters(_rsaParameters);
            _nonce = nonce;
            _directory = directory;
            _kid = kid;

            Payload = payload;
        }

        public JwsContainer(RSAParameters rsaParameters, string nonce, string directory, TPayload payload = null)
            :this(rsaParameters, nonce, directory, string.Empty, payload)
        { }

        public string SerializeSignedToken()
        {
            string token = string.Empty;

            if (Payload == null)
                throw new ArgumentException("Payload must be set before the token can be created and signed.");

            JWK jwk = null;
            string kid = null;

            if (string.IsNullOrEmpty(_kid))
            {
                //Create the JWK
                jwk = new JWK()
                {
                    e = Base64Tool.Encode(_rsaParameters.Exponent),
                    kty = "RSA",
                    n = Base64Tool.Encode(_rsaParameters.Modulus)
                };
            }
            else
            {
                kid = _kid;
            }

            //Create the Protected Header
            PROTECTED @protected = new PROTECTED()
            {
                alg = "RS256",
                jwk = jwk,
                kid = kid,
                nonce = _nonce,
                url = _directory
            };

            //Encode jwk and payload
            string encodedProtected = Base64Tool.Encode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@protected, Formatting.None)));
            string encodedPayload = Base64Tool.Encode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Payload, Formatting.None)));

            //Sign Token
            string sigBase = $"{encodedProtected}.{encodedPayload}";
            byte[] sigBytes = Encoding.ASCII.GetBytes(sigBase);
            byte[] signedBytes = _cryptoProvider.SignData(sigBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            string signature = Base64Tool.Encode(signedBytes);

            token = JsonConvert.SerializeObject(new
            {
                @protected = encodedProtected,
                payload = encodedPayload,
                signature = signature
            });

            return token;
        }

        private class JWK
        {
            public string e { get; set; }

            public string kty { get; set; }

            public string n { get; set; }
        }

        private class PROTECTED
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
}
