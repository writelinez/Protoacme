using Protoacme.Core;
using Protoacme.Core.Abstractions;
using Protoacme.Core.Enumerations;
using Protoacme.Core.Exceptions;
using Protoacme.Core.InternalRepositories;
using Protoacme.Models;
using Protoacme.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Protoacme
{
    /// <summary>
    /// ACME protocol client.
    /// </summary>
    public class ProtoacmeClient
    {
        private readonly IAcmeRestApi _acmeApi;
        private readonly ICachedRepository<AcmeDirectory> _directoryCache;
        private readonly ICachedRepository<string> _nonceCache;

        private readonly AcmeAccountService _accountService = null;
        private readonly AcmeCertificateService _certificateService = null;
        private readonly AcmeChallengeService _challengeService = null;

        /// <summary>
        /// Handles account related actions.
        /// </summary>
        public AcmeAccountService Account { get { return _accountService; } }

        /// <summary>
        /// Certificate actions
        /// </summary>
        public AcmeCertificateService Certificate { get { return _certificateService; } }

        /// <summary>
        /// Challenge actions such as certificate verification.
        /// </summary>
        public AcmeChallengeService Challenge { get { return _challengeService; } }

        public ProtoacmeClient(IAcmeRestApi acmeApi)
        {
            _acmeApi = acmeApi;

            _directoryCache = new CachedRepository<AcmeDirectory>(GetDirectory);
            _nonceCache = new CachedRepository<string>(GetNewNonce);

            _accountService = new AcmeAccountService(_acmeApi, _directoryCache, _nonceCache);
            _certificateService = new AcmeCertificateService(_acmeApi, _directoryCache, _nonceCache);
            _challengeService = new AcmeChallengeService(_acmeApi, _directoryCache, _nonceCache);
        }

        public ProtoacmeClient(string letsEncryptEndpoint)
            : this(new AcmeRestApi(letsEncryptEndpoint))
        { }

        public ProtoacmeClient()
            :this(new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_PROD_ENDPOINT))
        { }
        

        #region Private Functions
        private async Task<AcmeDirectory> GetDirectory()
        {
            var directoryResponse = await _acmeApi.GetDirectoryAsync();
            if (directoryResponse.Status == AcmeApiResponseStatus.Error)
                throw new AcmeProtocolException(directoryResponse.Message);

            return directoryResponse.Data;
        }

        private async Task<string> GetNewNonce()
        {
            var directory = await _directoryCache.GetAsync();
            var nonceResponse = await _acmeApi.GetNonceAsync(directory);
            if (nonceResponse.Status == AcmeApiResponseStatus.Error)
                throw new AcmeProtocolException(nonceResponse.Message);

            return nonceResponse.Nonce;
        }
        #endregion
    }
}
