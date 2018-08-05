using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Protoacme.Core.Enumerations;
using Protoacme.Models;

namespace Protoacme.Core.Abstractions
{
    public interface IAcmeRestApi
    {
        Task<AcmeApiResponse<AcmeAccount>> CreateAccount(AcmeDirectory directory, string nonce, AcmeCreateAccount account);
        Task<AcmeApiResponse> DeactivateAccount(AcmeDirectory directory, string nonce, AcmeAccount account);
        Task<AcmeApiResponse<AcmeCertificateFulfillmentPromise>> FinalizeChallenge(AcmeAccount account, string nonce, AcmeCertificateFulfillmentPromise acmeCertificateFulfillmentPromise, string csr);
        Task<AcmeApiResponse<ArraySegment<byte>>> GetCertificate(AcmeCertificateFulfillmentPromise completedPromise, CertificateType certificateType);
        Task<List<AcmeApiResponse<AcmeAuthorization>>> GetChallenges(AcmeCertificateFulfillmentPromise acmeCertificateFulfillmentPromise);
        Task<AcmeApiResponse<AcmeChallengeVerificationStatus>> GetChallengeVerificationStatus(AcmeChallenge challenge);
        Task<AcmeApiResponse<AcmeDirectory>> GetDirectoryAsync();
        Task<AcmeApiResponse> GetNonce(AcmeDirectory directory);
        Task<AcmeApiResponse<AcmeCertificateFulfillmentPromise>> RequestCertificate(AcmeDirectory directory, string nonce, AcmeAccount account, AcmeCertificateRequest certificates);
        Task<AcmeApiResponse> RollOverAccountKey(AcmeDirectory directory, string nonce, AcmeAccount account);
        Task<AcmeApiResponse> UpdateAccount(AcmeDirectory directory, string nonce, AcmeAccount account);
        Task<AcmeApiResponse<AcmeChallengeStatus>> VerifyChallenge(AcmeAccount account, AcmeChallenge challenge, string nonce, string keyAuthorization);
    }
}