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
    public class ProtoAcmeClient
    {
        private readonly IAcmeRestApi _acmeApi;
        private readonly ICachedRepository<AcmeDirectory> _directoryCache;
        private readonly ICachedRepository<string> _nonceCache;

        private readonly AcmeAccountService _accountService = null;

        public AcmeAccountService Account { get { return _accountService; } }

        public ProtoAcmeClient(IAcmeRestApi acmeApi)
        {
            _acmeApi = acmeApi;

            _directoryCache = new CachedRepository<AcmeDirectory>(GetDirectory);
            _nonceCache = new CachedRepository<string>(GetNewNonce);

            _accountService = new AcmeAccountService(_acmeApi, _directoryCache, _nonceCache);
        }

        public ProtoAcmeClient(string letsEncryptEndpoint)
            : this(new AcmeRestApi(letsEncryptEndpoint))
        { }

        public ProtoAcmeClient()
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
