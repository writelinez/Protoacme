using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Protoacme.Core.Enumerations;
using Protoacme.Models;

namespace Protoacme.Core.Abstractions
{
    public interface IAcmeRestApi
    {
        Task<AcmeApiResponse<AcmeAccount>> CreateAccountAsync(AcmeDirectory directory, string nonce, AcmeCreateAccount account);
        Task<AcmeApiResponse> DeactivateAccountAsync(AcmeDirectory directory, string nonce, AcmeAccount account);
        Task<AcmeApiResponse<AcmeCertificateFulfillmentPromise>> FinalizeCertificatePromiseAsync(AcmeAccount account, string nonce, AcmeCertificateFulfillmentPromise acmeCertificateFulfillmentPromise, string csr);
        Task<AcmeApiResponse<ArraySegment<byte>>> GetCertificateAsync(AcmeCertificateFulfillmentPromise completedPromise, CertificateType certificateType);
        Task<List<AcmeApiResponse<AcmeAuthorization>>> GetChallengesAsync(AcmeCertificateFulfillmentPromise acmeCertificateFulfillmentPromise);
        Task<AcmeApiResponse<AcmeChallengeVerificationStatus>> GetChallengeVerificationStatusAsync(AcmeChallenge challenge);
        Task<AcmeApiResponse<AcmeDirectory>> GetDirectoryAsync();
        Task<AcmeApiResponse> GetNonceAsync(AcmeDirectory directory);
        Task<AcmeApiResponse<AcmeCertificateFulfillmentPromise>> RequestCertificateAsync(AcmeDirectory directory, string nonce, AcmeAccount account, AcmeCertificateRequest certificates);
        Task<AcmeApiResponse> RollOverAccountKeyAsync(AcmeDirectory directory, string nonce, AcmeAccount account);
        Task<AcmeApiResponse> UpdateAccountAsync(AcmeDirectory directory, string nonce, AcmeAccount account);
        Task<AcmeApiResponse<AcmeChallengeStatus>> VerifyChallengeAsync(AcmeAccount account, AcmeChallenge challenge, string nonce, string keyAuthorization);
    }
}