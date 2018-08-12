using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Protoacme.Core;
using Protoacme.Core.Enumerations;
using Protoacme.Core.Utilities;
using Protoacme.Models;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Protoacme.IntegrationTests.AcmeRestApiTests
{
    [TestClass]
    public class AcmeRestApiBasicTests
    {
        [TestMethod]
        public async Task GetDirectory_ShouldReturnValidDirectoryInfo()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directoryResponse;

            //EXECUTE
            directoryResponse = await api.GetDirectoryAsync();

            //ASSERT
            directoryResponse.Status.ShouldBe(AcmeApiResponseStatus.Success);
            directoryResponse.Data.ShouldNotBeNull();
            directoryResponse.Data.NewNoonce.ShouldNotBeNull();
            directoryResponse.Data.NewNoonce.Length.ShouldBeGreaterThan(0);
            directoryResponse.Data.NewAccount.ShouldNotBeNull();
            directoryResponse.Data.NewAccount.Length.ShouldBeGreaterThan(0);
        }

        [TestMethod]
        public async Task GetNewNonce_ShouldReturnNewReplayNonce()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directoryResponse;
            AcmeApiResponse nonceResponse = null;

            //EXECUTE
            directoryResponse = await api.GetDirectoryAsync();
            nonceResponse = await api.GetNonceAsync(directoryResponse.Data);

            //ASSERT
            nonceResponse.ShouldNotBeNull();
            nonceResponse.Status.ShouldBe(AcmeApiResponseStatus.Success);
            nonceResponse.Nonce.Length.ShouldBeGreaterThan(0);
        }

        [TestMethod]
        public async Task CreateAccount_ShouldSuccessfullyCreateNewAccount()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directory;
            AcmeApiResponse nonceResponse = null;
            AcmeApiResponse<AcmeAccount> accountResponse = null;

            //EXECUTE
            directory = await api.GetDirectoryAsync();
            nonceResponse = await api.GetNonceAsync(directory.Data);
            accountResponse = await api.CreateAccountAsync(directory.Data, nonceResponse.Nonce, new AcmeCreateAccount() { Contact = new List<string>() { "mailto:bob@toast.com" }, TermsOfServiceAgreed = true });

            //ASSERT
            accountResponse.ShouldNotBeNull();
            accountResponse.Status.ShouldBe(AcmeApiResponseStatus.Success);
            accountResponse.Data.KID.ShouldNotBeNull();
            accountResponse.Data.KID.Length.ShouldBeGreaterThan(0);
            accountResponse.Data.Contact.ShouldNotBeNull();
            accountResponse.Data.Contact.Count.ShouldBe(1);
            accountResponse.Data.SecurityInfo.ShouldNotBeNull();
        }

        [TestMethod]
        public async Task UpdateAccount_ShoudUpdateSuccessfully()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directory;
            AcmeApiResponse nonceResponse = null;
            AcmeApiResponse<AcmeAccount> accountResponse = null;
            AcmeApiResponse updateAccountResponse = null;

            //EXECUTE
            directory = await api.GetDirectoryAsync();
            nonceResponse = await api.GetNonceAsync(directory.Data);
            accountResponse = await api.CreateAccountAsync(directory.Data, nonceResponse.Nonce, new AcmeCreateAccount() { Contact = new List<string>() { "mailto:bob@toast.com" }, TermsOfServiceAgreed = true });
            updateAccountResponse = await api.UpdateAccountAsync(directory.Data, accountResponse.Nonce, accountResponse.Data);

            //ASSERT
            updateAccountResponse.ShouldNotBeNull();
            updateAccountResponse.Status.ShouldBe(AcmeApiResponseStatus.Success);
            updateAccountResponse.Nonce.Length.ShouldBeGreaterThan(0);
        }

        [TestMethod]
        public async Task RollOverAccountKey_ShoudChangeAccountKeySuccessfully()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directory;
            AcmeApiResponse nonceResponse = null;
            AcmeApiResponse<AcmeAccount> accountResponse = null;
            AcmeApiResponse rollOverAccountKeyResponse = null;

            //EXECUTE
            directory = await api.GetDirectoryAsync();
            nonceResponse = await api.GetNonceAsync(directory.Data);
            accountResponse = await api.CreateAccountAsync(directory.Data, nonceResponse.Nonce, new AcmeCreateAccount() { Contact = new List<string>() { "mailto:bob@toast.com" }, TermsOfServiceAgreed = true });
            rollOverAccountKeyResponse = await api.RollOverAccountKeyAsync(directory.Data, accountResponse.Nonce, accountResponse.Data);

            //ASSERT
            rollOverAccountKeyResponse.ShouldNotBeNull();
            rollOverAccountKeyResponse.Status.ShouldBe(AcmeApiResponseStatus.Success);
            rollOverAccountKeyResponse.Nonce.Length.ShouldBeGreaterThan(0);
        }

        [TestMethod]
        public async Task DeactivateAccount_ShouldDeactiveSuccessfully()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directory;
            AcmeApiResponse nonceResponse = null;
            AcmeApiResponse<AcmeAccount> accountResponse = null;
            AcmeApiResponse deactivateAccountResponse = null;

            //EXECUTE
            directory = await api.GetDirectoryAsync();
            nonceResponse = await api.GetNonceAsync(directory.Data);
            accountResponse = await api.CreateAccountAsync(directory.Data, nonceResponse.Nonce, new AcmeCreateAccount() { Contact = new List<string>() { "mailto:bob@toast.com" }, TermsOfServiceAgreed = true });
            deactivateAccountResponse = await api.DeactivateAccountAsync(directory.Data, accountResponse.Nonce, accountResponse.Data);

            //ASSERT
            deactivateAccountResponse.ShouldNotBeNull();
            deactivateAccountResponse.Status.ShouldBe(AcmeApiResponseStatus.Success);
            deactivateAccountResponse.Nonce.Length.ShouldBeGreaterThan(0);
        }

        [TestMethod]
        public async Task RequestCertificate_ShouldGetPromiseBack()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directory;
            AcmeApiResponse nonceResponse = null;
            AcmeApiResponse<AcmeAccount> accountResponse = null;
            AcmeApiResponse<AcmeCertificateFulfillmentPromise> certificateFulfillmentPromise = null;

            AcmeCertificateRequest certifcateRequest = new AcmeCertificateRequest()
            {
                Identifiers = new List<DnsCertificateIdentifier>()
                {
                    new DnsCertificateIdentifier()
                    {
                        Value = "taco.com"
                    },
                    new DnsCertificateIdentifier()
                    {
                        Value = "www.taco.com"
                    }
                }
            };

            //EXECUTE
            directory = await api.GetDirectoryAsync();
            nonceResponse = await api.GetNonceAsync(directory.Data);
            accountResponse = await api.CreateAccountAsync(directory.Data, nonceResponse.Nonce, new AcmeCreateAccount() { Contact = new List<string>() { "mailto:bob@toast.com" }, TermsOfServiceAgreed = true });
            certificateFulfillmentPromise = await api.RequestCertificateAsync(directory.Data, accountResponse.Nonce, accountResponse.Data, certifcateRequest);

            //ASSERT
            certificateFulfillmentPromise.ShouldNotBeNull();
            certificateFulfillmentPromise.Status.ShouldBe(AcmeApiResponseStatus.Success);
            certificateFulfillmentPromise.Data.ShouldNotBeNull();
            certificateFulfillmentPromise.Data.Status.ShouldBe("pending");
            certificateFulfillmentPromise.Data.Identifiers.ShouldNotBeNull();
            certificateFulfillmentPromise.Data.Identifiers.Count.ShouldBe(2);
            certificateFulfillmentPromise.Data.Authorizations.ShouldNotBeNull();
            certificateFulfillmentPromise.Data.Authorizations.Count.ShouldBe(2);
            certificateFulfillmentPromise.Data.Finalize.ShouldNotBeNull();
            certificateFulfillmentPromise.Data.Finalize.Length.ShouldBeGreaterThan(0);
        }

        [TestMethod]
        public async Task GetChallenges_ShouldGetChallengesForEachIdentifier()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directory;
            AcmeApiResponse nonceResponse = null;
            AcmeApiResponse<AcmeAccount> accountResponse = null;
            AcmeApiResponse<AcmeCertificateFulfillmentPromise> certificateFulfillmentPromise = null;
            List<AcmeApiResponse<AcmeAuthorization>> authorizations = null;

            AcmeCertificateRequest certifcateRequest = new AcmeCertificateRequest()
            {
                Identifiers = new List<DnsCertificateIdentifier>()
                {
                    new DnsCertificateIdentifier()
                    {
                        Value = "taco.com"
                    },
                    new DnsCertificateIdentifier()
                    {
                        Value = "www.taco.com"
                    }
                }
            };

            //EXECUTE
            directory = await api.GetDirectoryAsync();
            nonceResponse = await api.GetNonceAsync(directory.Data);
            accountResponse = await api.CreateAccountAsync(directory.Data, nonceResponse.Nonce, new AcmeCreateAccount() { Contact = new List<string>() { "mailto:bob@toast.com" }, TermsOfServiceAgreed = true });
            certificateFulfillmentPromise = await api.RequestCertificateAsync(directory.Data, accountResponse.Nonce, accountResponse.Data, certifcateRequest);
            authorizations = await api.GetChallengesAsync(certificateFulfillmentPromise.Data);

            //ASSERT
            authorizations.ShouldNotBeNull();
            authorizations.Count.ShouldBe(2);
            authorizations.ShouldAllBe(auth => auth.Status == AcmeApiResponseStatus.Success);
        }

        [TestMethod]
        public async Task VerifyChallenge_ShouldVerifyTheChallenge()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directory;
            AcmeApiResponse nonceResponse = null;
            AcmeApiResponse<AcmeAccount> accountResponse = null;
            AcmeApiResponse<AcmeCertificateFulfillmentPromise> certificateFulfillmentPromise = null;
            List<AcmeApiResponse<AcmeAuthorization>> authorizations = null;
            AcmeApiResponse<AcmeChallengeStatus> challengeStatusResponse;

            AcmeCertificateRequest certifcateRequest = new AcmeCertificateRequest()
            {
                Identifiers = new List<DnsCertificateIdentifier>()
                {
                    new DnsCertificateIdentifier()
                    {
                        Value = "taco.com"
                    },
                    new DnsCertificateIdentifier()
                    {
                        Value = "www.taco.com"
                    }
                }
            };

            //EXECUTE
            directory = await api.GetDirectoryAsync();
            nonceResponse = await api.GetNonceAsync(directory.Data);
            accountResponse = await api.CreateAccountAsync(directory.Data, nonceResponse.Nonce, new AcmeCreateAccount() { Contact = new List<string>() { "mailto:bob@toast.com" }, TermsOfServiceAgreed = true });
            certificateFulfillmentPromise = await api.RequestCertificateAsync(directory.Data, accountResponse.Nonce, accountResponse.Data, certifcateRequest);
            authorizations = await api.GetChallengesAsync(certificateFulfillmentPromise.Data);

            AcmeChallenge httpChallenge = authorizations.First().Data.Challenges.First(t => t.Type.Equals("http-01"));
            string authKey = CreateAuthorizationKey(accountResponse.Data, httpChallenge.Token);

            challengeStatusResponse = await api.VerifyChallengeAsync(accountResponse.Data, httpChallenge, certificateFulfillmentPromise.Nonce, authKey);

            //ASSERT
            challengeStatusResponse.ShouldNotBeNull();
            challengeStatusResponse.Status.ShouldBe(AcmeApiResponseStatus.Success);
            challengeStatusResponse.Data.ShouldNotBeNull();
            challengeStatusResponse.Data.Status.ShouldBe("pending");
        }

        [TestMethod]
        public async Task GetChallengeVerificationStatus_ShouldComplete()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directory;
            AcmeApiResponse nonceResponse = null;
            AcmeApiResponse<AcmeAccount> accountResponse = null;
            AcmeApiResponse<AcmeCertificateFulfillmentPromise> certificateFulfillmentPromise = null;
            List<AcmeApiResponse<AcmeAuthorization>> authorizations = null;
            AcmeApiResponse<AcmeChallengeStatus> challengeStatusResponse = null;
            AcmeApiResponse<AcmeChallengeVerificationStatus> challengeVerificationResponse = null;

            AcmeCertificateRequest certifcateRequest = new AcmeCertificateRequest()
            {
                Identifiers = new List<DnsCertificateIdentifier>()
                {
                    new DnsCertificateIdentifier()
                    {
                        Value = "taco.com"
                    }
                }
            };

            //EXECUTE
            directory = await api.GetDirectoryAsync();
            nonceResponse = await api.GetNonceAsync(directory.Data);
            accountResponse = await api.CreateAccountAsync(directory.Data, nonceResponse.Nonce, new AcmeCreateAccount() { Contact = new List<string>() { "mailto:bob@toast.com" }, TermsOfServiceAgreed = true });
            certificateFulfillmentPromise = await api.RequestCertificateAsync(directory.Data, accountResponse.Nonce, accountResponse.Data, certifcateRequest);
            authorizations = await api.GetChallengesAsync(certificateFulfillmentPromise.Data);

            AcmeChallenge httpChallenge = authorizations.First().Data.Challenges.First(t => t.Type.Equals("http-01"));
            string authKey = CreateAuthorizationKey(accountResponse.Data, httpChallenge.Token);

            challengeStatusResponse = await api.VerifyChallengeAsync(accountResponse.Data, httpChallenge, certificateFulfillmentPromise.Nonce, authKey);
            challengeVerificationResponse = await api.GetChallengeVerificationStatusAsync(httpChallenge);

            //ASSERT
            challengeVerificationResponse.ShouldNotBeNull();
            challengeVerificationResponse.Status.ShouldBe(AcmeApiResponseStatus.Success);
        }

        [TestMethod]
        public async Task FinalizeChallenge_ShouldComplete()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directory;
            AcmeApiResponse nonceResponse = null;
            AcmeApiResponse<AcmeAccount> accountResponse = null;
            AcmeApiResponse<AcmeCertificateFulfillmentPromise> certificateFulfillmentPromise = null;
            List<AcmeApiResponse<AcmeAuthorization>> authorizations = null;
            AcmeApiResponse<AcmeChallengeStatus> challengeStatusResponse = null;
            AcmeApiResponse<AcmeChallengeVerificationStatus> challengeVerificationResponse = null;
            AcmeApiResponse<AcmeCertificateFulfillmentPromise> certificatePromiseResult = null;

            AcmeCertificateRequest certifcateRequest = new AcmeCertificateRequest()
            {
                Identifiers = new List<DnsCertificateIdentifier>()
                {
                    new DnsCertificateIdentifier()
                    {
                        Value = "test.com"
                    }
                }
            };

            //EXECUTE
            directory = await api.GetDirectoryAsync();
            nonceResponse = await api.GetNonceAsync(directory.Data);
            accountResponse = await api.CreateAccountAsync(directory.Data, nonceResponse.Nonce, new AcmeCreateAccount() { Contact = new List<string>() { "mailto:bob@toast.com" }, TermsOfServiceAgreed = true });
            certificateFulfillmentPromise = await api.RequestCertificateAsync(directory.Data, accountResponse.Nonce, accountResponse.Data, certifcateRequest);
            authorizations = await api.GetChallengesAsync(certificateFulfillmentPromise.Data);

            AcmeChallenge httpChallenge = authorizations.First().Data.Challenges.First(t => t.Type.Equals("http-01"));
            string authKey = CreateAuthorizationKey(accountResponse.Data, httpChallenge.Token);

            challengeStatusResponse = await api.VerifyChallengeAsync(accountResponse.Data, httpChallenge, certificateFulfillmentPromise.Nonce, authKey);
            while (
                challengeVerificationResponse == null || 
                challengeVerificationResponse.Data.Status == "pending")
            {
                challengeVerificationResponse = await api.GetChallengeVerificationStatusAsync(httpChallenge);
                await Task.Delay(3000);
            }

            string csr = GenerateCSR(accountResponse.Data, "test.com");

            certificatePromiseResult = await api.FinalizeCertificatePromiseAsync(accountResponse.Data, challengeStatusResponse.Nonce, certificateFulfillmentPromise.Data, csr);

            //ASSERT (Cant really assert anything here. This call will mostlikey fail. There is no way to validate the domain here)
            certificatePromiseResult.ShouldNotBeNull();
        }

        [TestMethod]
        public async Task DownloadCertificate_ShouldComplete()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directory;
            AcmeApiResponse nonceResponse = null;
            AcmeApiResponse<AcmeAccount> accountResponse = null;
            AcmeApiResponse<AcmeCertificateFulfillmentPromise> certificateFulfillmentPromise = null;
            List<AcmeApiResponse<AcmeAuthorization>> authorizations = null;
            AcmeApiResponse<AcmeChallengeStatus> challengeStatusResponse = null;
            AcmeApiResponse<AcmeChallengeVerificationStatus> challengeVerificationResponse = null;
            AcmeApiResponse<AcmeCertificateFulfillmentPromise> certificatePromiseResult = null;
            AcmeApiResponse<ArraySegment<byte>> certificateResult = null;

            AcmeCertificateRequest certifcateRequest = new AcmeCertificateRequest()
            {
                Identifiers = new List<DnsCertificateIdentifier>()
                {
                    new DnsCertificateIdentifier()
                    {
                        Value = "test.com"
                    }
                }
            };

            //EXECUTE
            directory = await api.GetDirectoryAsync();
            nonceResponse = await api.GetNonceAsync(directory.Data);
            accountResponse = await api.CreateAccountAsync(directory.Data, nonceResponse.Nonce, new AcmeCreateAccount() { Contact = new List<string>() { "mailto:bob@toast.com" }, TermsOfServiceAgreed = true });
            certificateFulfillmentPromise = await api.RequestCertificateAsync(directory.Data, accountResponse.Nonce, accountResponse.Data, certifcateRequest);
            authorizations = await api.GetChallengesAsync(certificateFulfillmentPromise.Data);

            AcmeChallenge httpChallenge = authorizations.First().Data.Challenges.First(t => t.Type.Equals("http-01"));
            string authKey = CreateAuthorizationKey(accountResponse.Data, httpChallenge.Token);

            challengeStatusResponse = await api.VerifyChallengeAsync(accountResponse.Data, httpChallenge, certificateFulfillmentPromise.Nonce, authKey);
            while (
                challengeVerificationResponse == null ||
                challengeVerificationResponse.Data.Status == "pending")
            {
                challengeVerificationResponse = await api.GetChallengeVerificationStatusAsync(httpChallenge);
                await Task.Delay(3000);
            }

            string csr = GenerateCSR(accountResponse.Data, "test.com");

            certificatePromiseResult = await api.FinalizeCertificatePromiseAsync(accountResponse.Data, challengeStatusResponse.Nonce, certificateFulfillmentPromise.Data, csr);
            certificateResult = await api.GetCertificateAsync(certificatePromiseResult.Data, CertificateType.Cert);

            //We will write the cert out to a temp directory if it exists. Otherwise, forget it.
            if (Directory.Exists(@"c:\temp"))
            {
                using (FileStream fs = new FileStream(@"c:\temp\mycert.cer", FileMode.Create))
                {
                    byte[] bytes = certificateResult.Data.Array;
                    fs.Write(bytes, 0, bytes.Length);
                }
            }

            //ASSERT (Cant really assert anything here. This call will mostlikey fail. There is no way to validate the domain here)
            
        }

        private string CreateAuthorizationKey(AcmeAccount account, string challengeToken)
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

        private string GenerateCSR(AcmeAccount account, params string[] domainNames)
        {
            HashAlgorithmName hashName = HashAlgorithmName.SHA256;

            var builder = new SubjectAlternativeNameBuilder();
            foreach (var name in domainNames)
            {
                builder.AddDnsName(name);
            }

            RSA rsa = RSA.Create(4096);
            //rsa.ImportParameters(account.SecurityInfo);

            var dn = new X500DistinguishedName($"CN={domainNames.First()}");
            var csr = new CertificateRequest(dn, rsa, hashName, RSASignaturePadding.Pkcs1);
            csr.CertificateExtensions.Add(builder.Build());

            return Base64Tool.Encode(csr.CreateSigningRequest());
        }
    }
}
