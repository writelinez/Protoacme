using Protoacme.Core;
using Protoacme.Core.Abstractions;
using Protoacme.Core.Enumerations;
using Protoacme.Core.Exceptions;
using Protoacme.Core.InternalRepositories;
using Protoacme.Models;
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

        public ProtoAcmeClient(IAcmeRestApi acmeApi)
        {
            _acmeApi = acmeApi;

            _directoryCache = new CachedRepository<AcmeDirectory>(GetDirectory);
            _nonceCache = new CachedRepository<string>(GetNewNonce);
        }

        public ProtoAcmeClient(string letsEncryptEndpoint)
            : this(new AcmeRestApi(letsEncryptEndpoint))
        { }

        public ProtoAcmeClient()
            :this(new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_PROD_ENDPOINT))
        { }

        public async Task<AcmeAccount> CreateAccountAsync(AcmeCreateAccount accountDetails)
        {
            var directory = await _directoryCache.GetAsync();
            var nonce = await _nonceCache.GetAsync();

            var accountResponse = await _acmeApi.CreateAccountAsync(directory, nonce, accountDetails);
            if (accountResponse.Status == AcmeApiResponseStatus.Error)
                throw new AcmeProtocolException(accountResponse.Message);

            _nonceCache.Update(accountResponse.Nonce);

            return accountResponse.Data;
        }

        

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
