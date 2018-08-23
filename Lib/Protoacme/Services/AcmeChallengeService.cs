using Protoacme.Challenge;
using Protoacme.Core;
using Protoacme.Core.Abstractions;
using Protoacme.Core.Enumerations;
using Protoacme.Core.Exceptions;
using Protoacme.Models;
using Protoacme.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protoacme.Services
{
    public class AcmeChallengeService
    {
        private readonly IAcmeRestApi _acmeApi;
        private readonly ICachedRepository<AcmeDirectory> _directoryCache;
        private readonly ICachedRepository<string> _nonceCache;

        public AcmeChallengeService(IAcmeRestApi acmeApi, ICachedRepository<AcmeDirectory> directoryCache, ICachedRepository<string> nonceCache)
        {
            _acmeApi = acmeApi;
            _directoryCache = directoryCache;
            _nonceCache = nonceCache;
        }

        /// <summary>
        /// Gets challenges used to verify domain ownership.
        /// </summary>
        /// <param name="account">Existing account.</param>
        /// <param name="acmeCertificateFulfillmentPromise">The certificate fulfillment promise retrieved from the RequestCertificate call.</param>
        /// <param name="challengeType">The challenge type expected back.</param>
        /// <returns>Challenge used to verify domain ownership</returns>
        /// <remarks>If requesting a challenge for a wildcard domain, only dns challenge is supported.</remarks>
        /// <exception cref="NotSupportedException">If the challenge type is not supported.</exception>
        /// <exception cref="AcmeProtocolException">On all other Acme related exceptions</exception>
        public async Task<ChallengeCollection> GetChallengesAsync(AcmeAccount account, AcmeCertificateFulfillmentPromise acmeCertificateFulfillmentPromise, ChallengeType challengeType)
        {
            var response = await _acmeApi.GetChallengesAsync(acmeCertificateFulfillmentPromise);
            var errorResponse = response.Where(t => t.Status == AcmeApiResponseStatus.Error);
            if (errorResponse.Any())
                throw new AcmeProtocolException(string.Join(" | ", errorResponse.Select(t => t.Message)));

            ChallengeCollection challenges = new ChallengeCollection();

            foreach (var resp in response)
            {
                AcmeChallenge sChallenge = resp.Data.Challenges.FirstOrDefault(t => t.Type.Equals(challengeType.Value));
                if (sChallenge == null)
                    throw new NotSupportedException($"{challengeType.Value} challenge type not supported in this context.");
                IAcmeChallengeContent challengeContent = null;
                switch (challengeType.Value)
                {
                    case ProtoacmeContants.CHALLENGE_HTTP:
                        challengeContent = new HttpChallenge(account, sChallenge);
                        challenges.Add(challengeContent);
                        break;
                    case ProtoacmeContants.CHALLENGE_DNS:
                        challengeContent = new DnsChallenge(account, sChallenge);
                        challenges.Add(challengeContent);
                        break;
                    case ProtoacmeContants.CHALLENGE_TLS:
                        challengeContent = new TlsChallenge(account, sChallenge);
                        challenges.Add(challengeContent);
                        break;
                    default:
                        break;
                }
            }

            return challenges;
        }

        /// <summary>
        /// Tells the Acme server to start the verification process for the challenge
        /// </summary>
        /// <param name="challenge">The challenge to start the verification process for</param>
        /// <returns>The challenge status.</returns>
        public async Task<AcmeChallengeStatus> ExecuteChallengeVerification(IAcmeChallengeContent challenge)
        {
            var nonce = await _nonceCache.GetAsync();

            var response = await _acmeApi.VerifyChallengeAsync(challenge.Account, challenge.Challenge, nonce, challenge.AuthorizationKey);
            if (response.Status == AcmeApiResponseStatus.Error)
                throw new AcmeProtocolException(response.Message);

            _nonceCache.Update(response.Nonce);

            return response.Data;
        }

        /// <summary>
        /// Gets the status of the challenge verification.
        /// </summary>
        /// <param name="challenge">The challenge to check verification on</param>
        /// <returns>The challenge verification status.</returns>
        /// <remarks>Do not spam this call. Try to space out your calls by 3 seconds at a minimum.</remarks>
        public async Task<AcmeChallengeVerificationStatus> GetChallengeVerificationStatus(IAcmeChallengeContent challenge)
        {
            var response = await _acmeApi.GetChallengeVerificationStatusAsync(challenge.Challenge);
            if (response.Status == AcmeApiResponseStatus.Error)
                throw new AcmeProtocolException(response.Message);

            return response.Data;
        }
    }
}
