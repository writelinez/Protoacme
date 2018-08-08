﻿using Protoacme.Challenge;
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

        public async Task<IEnumerable<IAcmeChallengeContent>> GetChallenge(AcmeAccount account, AcmeCertificateFulfillmentPromise acmeCertificateFulfillmentPromise, ChallengeType challengeType)
        {
            var response = await _acmeApi.GetChallengesAsync(acmeCertificateFulfillmentPromise);
            var errorResponse = response.Where(t => t.Status == AcmeApiResponseStatus.Error);
            if (errorResponse.Any())
                throw new AcmeProtocolException(string.Join(" | ", errorResponse.Select(t => t.Message)));

            List<IAcmeChallengeContent> challenges = new List<IAcmeChallengeContent>();

            foreach (var resp in response)
            {
                AcmeChallenge sChallenge = resp.Data.Challenges.FirstOrDefault(t => t.Type.Equals(challengeType.Value));
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

        public async Task<AcmeChallengeStatus> ExecuteChallengeVerification(IAcmeChallengeContent challenge)
        {
            var nonce = await _nonceCache.GetAsync();

            var response = await _acmeApi.VerifyChallengeAsync(challenge.Account, challenge.Challenge, nonce, challenge.AuthorizationKey);
            if (response.Status == AcmeApiResponseStatus.Error)
                throw new AcmeProtocolException(response.Message);

            _nonceCache.Update(response.Nonce);

            return response.Data;
        }

        public async Task<AcmeChallengeVerificationStatus> GetChallengeVerificationStatus(IAcmeChallengeContent challenge)
        {
            var response = await _acmeApi.GetChallengeVerificationStatusAsync(challenge.Challenge);
            if (response.Status == AcmeApiResponseStatus.Error)
                throw new AcmeProtocolException(response.Message);

            return response.Data;
        }
    }
}