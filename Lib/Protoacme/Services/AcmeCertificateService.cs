using Protoacme.Core.Abstractions;
using Protoacme.Core.Enumerations;
using Protoacme.Core.Exceptions;
using Protoacme.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Protoacme.Services
{
    public class AcmeCertificateService
    {
        private readonly IAcmeRestApi _acmeApi;
        private readonly ICachedRepository<AcmeDirectory> _directoryCache;
        private readonly ICachedRepository<string> _nonceCache;

        public AcmeCertificateService(IAcmeRestApi acmeApi, ICachedRepository<AcmeDirectory> directoryCache, ICachedRepository<string> nonceCache)
        {
            _acmeApi = acmeApi;
            _directoryCache = directoryCache;
            _nonceCache = nonceCache;
        }

        public async Task<AcmeCertificateFulfillmentPromise> RequestCertificateAsync(AcmeAccount account, AcmeCertificateRequest certificate)
        {
            var directory = await _directoryCache.GetAsync();
            var nonce = await _nonceCache.GetAsync();

            var response = await _acmeApi.RequestCertificateAsync(directory, nonce, account, certificate);
            if (response.Status == AcmeApiResponseStatus.Error)
                throw new AcmeProtocolException(response.Message);

            _nonceCache.Update(response.Nonce);

            return response.Data;
        }

        public async Task<ArraySegment<byte>> DownloadCertificate(AcmeCertificateFulfillmentPromise completedPromise, CertificateType certificateType)
        {
            if (string.IsNullOrEmpty(completedPromise.Certificate))
                throw new AcmeProtocolException("Acme Certificate Fulfillment Promise has not been completed. You must complete a challenge before you can download your certificate.");

            var directory = await _directoryCache.GetAsync();
            var nonce = await _nonceCache.GetAsync();

            var response = await _acmeApi.GetCertificateAsync(completedPromise, certificateType);
            if (response.Status == AcmeApiResponseStatus.Error)
                throw new AcmeProtocolException(response.Message);

            _nonceCache.Update(response.Nonce);

            return response.Data;
        }
    }
}
