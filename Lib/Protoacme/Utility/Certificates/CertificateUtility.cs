using Newtonsoft.Json;
using Protoacme.Core.Utilities;
using Protoacme.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Protoacme.Utility.Certificates
{
    public static class CertificateUtility
    {
        public static string CreateAuthorizationKey(AcmeAccount account, string challengeToken)
        {
            string jwkThumbprint = string.Empty;

            //Compute the JWK Thumbprint
            var jwk = new
            {
                e = Base64Tool.Encode(account.SecurityInfo.Exponent),
                kty = "RSA",
                n = Base64Tool.Encode(account.SecurityInfo.Modulus)
            };

            string sjwk = JsonConvert.SerializeObject(jwk, Formatting.None);

            using (HashAlgorithm sha = SHA256.Create())
            {
                byte[] bjwk = Encoding.UTF8.GetBytes(sjwk);
                jwkThumbprint = Base64Tool.Encode(sha.ComputeHash(bjwk));
            }

            return $"{challengeToken}.{jwkThumbprint}";
        }

        public static CSR GenerateCsr(params string[] dnsNames)
        {
            HashAlgorithmName hashName = HashAlgorithmName.SHA256;
            var builder = new SubjectAlternativeNameBuilder();
            foreach (var name in dnsNames)
            {
                builder.AddDnsName(name);
            }

            RSA rsa = RSA.Create(4096);

            var dn = new X500DistinguishedName($"CN={dnsNames.First()}");
            var csr = new CertificateRequest(dn, rsa, hashName, RSASignaturePadding.Pkcs1);
            csr.CertificateExtensions.Add(builder.Build());

            return new CSR(csr.CreateSigningRequest());
        }

        public static CSR ImportCSR(byte[] buffer)
        {
            var cert = X509Certificate.CreateFromCertFile("");
            return null;
        }
    }
}
