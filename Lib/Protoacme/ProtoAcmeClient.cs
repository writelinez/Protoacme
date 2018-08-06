using Protoacme.Core;
using Protoacme.Core.Abstractions;
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

        public ProtoAcmeClient(IAcmeRestApi acmeApi)
        {
            _acmeApi = acmeApi;
        }

        public ProtoAcmeClient(string letsEncryptEndpoint)
            : this(new AcmeRestApi(letsEncryptEndpoint))
        { }

        public ProtoAcmeClient()
            :this(new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_PROD_ENDPOINT))
        { }

        public async Task<AcmeAccount> CreateAccount(AcmeCreateAccount accountDetails)
        {
            var directory = await _acmeApi.GetDirectoryAsync();
            return null;
        }
    }
}
