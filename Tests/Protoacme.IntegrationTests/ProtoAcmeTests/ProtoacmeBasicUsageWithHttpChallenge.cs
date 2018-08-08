using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Protoacme.Challenge;
using Protoacme.Core;
using Protoacme.Core.Abstractions;
using Protoacme.Models;
using Protoacme.Utility;
using Protoacme.Utility.Certificates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Protoacme.IntegrationTests.ProtoAcmeTests
{
    [TestClass]
    public class ProtoacmeBasicUsageWithHttpChallenge
    {
        private List<string> dnsNames = new List<string>()
        {
            "vapedish.com",
            "www.vapedish.com"
        };

        [TestMethod]
        public async Task StandardCertificateRequest()
        {
            var restApi = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            var client = new ProtoacmeClient(restApi);

            //1. Create a new account.
            var newAccountInfo = new AcmeCreateAccount();
            newAccountInfo.Contact.Add("mailto:bob@gmail.com");
            newAccountInfo.Contact.Add("mailto:toast@yahoo.com");
            newAccountInfo.TermsOfServiceAgreed = true;

            var account = await client.Account.CreateAsync(newAccountInfo);

            //2. Request a certificate.
            AcmeCertificateRequest certRequest = new AcmeCertificateRequest();
            foreach (var dns in dnsNames)
            {
                certRequest.Identifiers.Add(new DnsCertificateIdentifier() { Value = dns });
            }
            var certPromise = await client.Certificate.RequestCertificateAsync(account, certRequest);


            //3. Get challenge
            var challenges = await client.Challenge.GetChallenge(account, certPromise, ChallengeType.Http);

            //4. Save Challenge and Account for next step.
            SaveAccountAndChallengeData(account, challenges, certPromise);
            
        }

        [TestMethod]
        public async Task IssueStandardCertificates()
        {
            var restApi = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            var client = new ProtoacmeClient(restApi);

            LoadAccountAndChallengeData<HttpChallenge>(out AcmeAccount account, out List<HttpChallenge> challenges, out AcmeCertificateFulfillmentPromise promise);

            //1. Verify all of the challenges
            foreach (var dnsChallenge in challenges)
            {
                var startVerifyResult = await client.Challenge.ExecuteChallengeVerification(dnsChallenge);
                AcmeChallengeStatus challengeStatus = null;
                while (challengeStatus == null || challengeStatus.Status == "pending")
                {
                    challengeStatus = await client.Challenge.GetChallengeVerificationStatus(dnsChallenge);
                }

                if (challengeStatus.Status != "valid")
                    throw new Exception($"Failed to validate challenge token {dnsChallenge.Token}");
            }

            //2. If everything is good we download the certificate
            CSR csr = CertificateUtility.GenerateCsr(dnsNames.ToArray());
                //Normally you would save the csr to be used next time.

            //client.Certificate.DownloadCertificate()

        }

        private void SaveAccountAndChallengeData(AcmeAccount account, IEnumerable<IAcmeChallengeContent> challenges, AcmeCertificateFulfillmentPromise promise)
        {
            string baseFolder = @"c:\temp";

            var serializedAccount = JsonConvert.SerializeObject(account, Formatting.None);
            serializedAccount = Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedAccount));
            using (FileStream fs = new FileStream(Path.Combine(baseFolder, "myaccount.acc"), FileMode.Create))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(serializedAccount);
                fs.Write(buffer, 0, buffer.Length);
            }

            var sChallenges = JsonConvert.SerializeObject(challenges, Formatting.None);
            sChallenges = Convert.ToBase64String(Encoding.UTF8.GetBytes(sChallenges));
            using (FileStream fs = new FileStream(Path.Combine(baseFolder, "challenges.dat"), FileMode.Create))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(sChallenges);
                fs.Write(buffer, 0, buffer.Length);
            }

            var sCertificatePromise = JsonConvert.SerializeObject(promise);
            sCertificatePromise = Convert.ToBase64String(Encoding.UTF8.GetBytes(sCertificatePromise));
            using (FileStream fs = new FileStream(Path.Combine(baseFolder, "challengepromise.dat"), FileMode.Create))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(sCertificatePromise);
                fs.Write(buffer, 0, buffer.Length);
            }

            foreach (var challenge in challenges)
            {
                challenge.SaveToFile(Path.Combine(baseFolder, challenge.Token));
            }
        }

        private void LoadAccountAndChallengeData<TChallengeType>(out AcmeAccount account, out List<TChallengeType> challenges, out AcmeCertificateFulfillmentPromise promise)
            where TChallengeType : IAcmeChallengeContent
        {
            string baseFolder = @"c:\temp";

            using (FileStream fs = new FileStream(Path.Combine(baseFolder, "myaccount.acc"), FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    var sAccount = sr.ReadToEnd();
                    var bAccount = Convert.FromBase64String(sAccount);
                    sAccount = Encoding.UTF8.GetString(bAccount);
                    account = JsonConvert.DeserializeObject<AcmeAccount>(sAccount);
                }
            }

            using (FileStream fs = new FileStream(Path.Combine(baseFolder, "challenges.dat"), FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    var sChallenges = sr.ReadToEnd();
                    var bChallenges = Convert.FromBase64String(sChallenges);
                    sChallenges = Encoding.UTF8.GetString(bChallenges);
                    challenges = JsonConvert.DeserializeObject<List<TChallengeType>>(sChallenges);
                }
            }

            using (FileStream fs = new FileStream(Path.Combine(baseFolder, "challengepromise.dat"), FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    var sPromise = sr.ReadToEnd();
                    var bPromise = Convert.FromBase64String(sPromise);
                    sPromise = Encoding.UTF8.GetString(bPromise);
                    promise = JsonConvert.DeserializeObject<AcmeCertificateFulfillmentPromise>(sPromise);
                }
            }
        }
    }
}
