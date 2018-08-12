using Protoacme.Core.Abstractions;
using Protoacme.Core.Enumerations;
using Protoacme.Core.Exceptions;
using Protoacme.Core.InternalRepositories;
using Protoacme.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Protoacme.Services
{
    public class AcmeAccountService
    {
        private readonly IAcmeRestApi _acmeApi;
        private readonly ICachedRepository<AcmeDirectory> _directoryCache;
        private readonly ICachedRepository<string> _nonceCache;

        public AcmeAccountService(IAcmeRestApi acmeApi, ICachedRepository<AcmeDirectory> directoryCache, ICachedRepository<string> nonceCache)
        {
            _acmeApi = acmeApi;
            _directoryCache = directoryCache;
            _nonceCache = nonceCache;
        }

        /// <summary>
        /// Create a new account.
        /// </summary>
        /// <param name="accountDetails">Information for new account.</param>
        /// <returns>Returns a serializable account object.</returns>
        public async Task<AcmeAccount> CreateAsync(AcmeCreateAccount accountDetails)
        {
            var directory = await _directoryCache.GetAsync();
            var nonce = await _nonceCache.GetAsync();

            var response = await _acmeApi.CreateAccountAsync(directory, nonce, accountDetails);
            if (response.Status == AcmeApiResponseStatus.Error)
                throw new AcmeProtocolException(response.Message);

            _nonceCache.Update(response.Nonce);

            return response.Data;
        }

        /// <summary>
        /// Updates an existing accounts contacts
        /// </summary>
        /// <param name="account">Existing account</param>
        /// <returns></returns>
        public async Task UpdateAsync(AcmeAccount account)
        {
            var directory = await _directoryCache.GetAsync();
            var nonce = await _nonceCache.GetAsync();

            var response = await _acmeApi.UpdateAccountAsync(directory, nonce, account);
            if (response.Status == AcmeApiResponseStatus.Error)
                throw new AcmeProtocolException(response.Message);

            _nonceCache.Update(response.Nonce);
        }

        /// <summary>
        /// Changes and updates the account security info for an existing account.
        /// </summary>
        /// <param name="account">Existing account</param>
        /// <returns></returns>
        /// <remarks>Will update the security info on the passed in account, so you will need to reserialize and update your existing account object to update the security info.</remarks>
        public async Task ChangeKeyAsync(AcmeAccount account)
        {
            var directory = await _directoryCache.GetAsync();
            var nonce = await _nonceCache.GetAsync();

            var response = await _acmeApi.RollOverAccountKeyAsync(directory, nonce, account);
            if (response.Status == AcmeApiResponseStatus.Error)
                throw new AcmeProtocolException(response.Message);

            _nonceCache.Update(response.Nonce);
        }

        /// <summary>
        /// Deactivates an existing account
        /// </summary>
        /// <param name="account">Existing account</param>
        /// <returns></returns>
        public async Task DeactiveAsync(AcmeAccount account)
        {
            var directory = await _directoryCache.GetAsync();
            var nonce = await _nonceCache.GetAsync();

            var response = await _acmeApi.DeactivateAccountAsync(directory, nonce, account);
            if (response.Status == AcmeApiResponseStatus.Error)
                throw new AcmeProtocolException(response.Message);

            _nonceCache.Update(response.Nonce);
        }
    }
}
