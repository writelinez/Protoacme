using Newtonsoft.Json;
using Protoacme.Core.Enumerations;
using Protoacme.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Protoacme.Core
{
    public class AcmeRestApi
    {
        private readonly HttpClient _httpClient = null;
        private readonly string _letsEncryptEndpoint = string.Empty;

        public AcmeRestApi(string letsEncryptEndpoint, HttpClient httpClient)
        {
            _letsEncryptEndpoint = letsEncryptEndpoint;
            _httpClient = httpClient;
        }

        public AcmeRestApi(string letsEncryptEndpoint)
            :this(letsEncryptEndpoint, new HttpClient())
        { }

        public async Task<AcmeApiResponse<AcmeDirectory>> GetDirectoryAsync()
        {
            var apiResp = await _httpClient.GetAsync(_letsEncryptEndpoint.AppendUrl(ProtoacmeContants.LETSENCRYPT_DIRECTORY_FRAGMENT));
            if (!apiResp.IsSuccessStatusCode)
                return ErrorResponse<AcmeDirectory>(await apiResp.Content?.ReadAsStringAsync());

            return new AcmeApiResponse<AcmeDirectory>()
            {
                Status = AcmeApiResponseStatus.Success,
                Data = JsonConvert.DeserializeObject<AcmeDirectory>(await apiResp.Content?.ReadAsStringAsync())
            };
        }

        public async Task<AcmeApiResponse> GetNonce(AcmeDirectory directory)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (string.IsNullOrEmpty(directory.NewNoonce))
                throw new ArgumentException("directory is missing NewNonce url.");

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Head, directory.NewNoonce);
            var apiResp = await _httpClient.SendAsync(msg);
            if (!apiResp.IsSuccessStatusCode)
                return ErrorResponse(await apiResp.Content?.ReadAsStringAsync());

            if (!apiResp.Headers.TryGetValues(ProtoacmeContants.HEADER_NONCE, out IEnumerable<string> values))
                return ErrorResponse("Call to get new nonce missing NONCE header.");

            return new AcmeApiResponse()
            {
                Status = AcmeApiResponseStatus.Success,
                Nonce = values.FirstOrDefault()
            };
        }

        public async Task<AcmeApiResponse<AcmeAccount>> CreateAccount(AcmeDirectory directory, string nonce, AcmeCreateAccount account)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (string.IsNullOrEmpty(directory.NewAccount))
                throw new ArgumentException("directory is missing Account url.");
            if (string.IsNullOrEmpty(nonce))
                throw new ArgumentNullException("nonce");
            if (account == null)
                throw new ArgumentNullException("account");

            RSACryptoServiceProvider cryptoProvider = new RSACryptoServiceProvider(2048);
            RSAParameters rsaPrams = cryptoProvider.ExportParameters(true);

            JwsContainer<AcmeCreateAccount> jwsObject = new JwsContainer<AcmeCreateAccount>(rsaPrams, nonce, directory.NewAccount, account);

            string jwsToken = jwsObject.SerializeSignedToken();

            StringContent content = new StringContent(jwsToken);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/jose+json");

            var newAccResponse = await _httpClient.PostAsync(directory.NewAccount, content);

            string newAccResponseString = await newAccResponse.Content?.ReadAsStringAsync();
            if (newAccResponse.StatusCode != HttpStatusCode.Created)
                return ErrorResponse<AcmeAccount>(newAccResponseString);

            if (!newAccResponse.Headers.TryGetValues(ProtoacmeContants.HEADER_LOCATION, out IEnumerable<string> locations))
                return ErrorResponse<AcmeAccount>("Missing Location Header on CreateAccount Response.");

            if (!newAccResponse.Headers.TryGetValues(ProtoacmeContants.HEADER_NONCE, out IEnumerable<string> nonces))
                return ErrorResponse<AcmeAccount>("Missing Replay-Nonce Header on CreateAccount Response.");

            Dictionary<string, object> oResp = JsonConvert.DeserializeObject<Dictionary<string, object>>(newAccResponseString);

            return new AcmeApiResponse<AcmeAccount>()
            {
                Status = AcmeApiResponseStatus.Success,
                Nonce = nonces.FirstOrDefault(),
                Data = new AcmeAccount()
                {
                    Id = Convert.ToInt32(oResp["id"]),
                    KID = locations.FirstOrDefault(),
                    SecurityInfo = rsaPrams,
                    Contact = account.Contact
                }
            };
        }

        public async Task<AcmeApiResponse> UpdateAccount(AcmeDirectory directory, string nonce, AcmeAccount account)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (string.IsNullOrEmpty(directory.NewAccount))
                throw new ArgumentException("directory is missing Account url.");
            if (string.IsNullOrEmpty(nonce))
                throw new ArgumentNullException("nonce");
            if (account == null)
                throw new ArgumentNullException("account");

            AcmeCreateAccount upd = new AcmeCreateAccount() { Contact = account.Contact, TermsOfServiceAgreed = true };

            JwsContainer<AcmeCreateAccount> jwsObject = new JwsContainer<AcmeCreateAccount>(account.SecurityInfo, nonce, account.KID, account.KID, upd);

            string jwsToken = jwsObject.SerializeSignedToken();

            StringContent content = new StringContent(jwsToken);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/jose+json");

            var newAccResponse = await _httpClient.PostAsync(_letsEncryptEndpoint.AppendUrl(ProtoacmeContants.LETSENCRYPT_ACCOUNT_FRAGMENT).AppendUrl(account.Id.ToString()), content);
            string newAccResponseString = await newAccResponse.Content?.ReadAsStringAsync();
            if (newAccResponse.StatusCode != HttpStatusCode.OK)
                return ErrorResponse(newAccResponseString);

            if (!newAccResponse.Headers.TryGetValues(ProtoacmeContants.HEADER_NONCE, out IEnumerable<string> nonces))
                return ErrorResponse<AcmeAccount>("Missing Replay-Nonce Header on CreateAccount Response.");

            return new AcmeApiResponse()
            {
                Status = AcmeApiResponseStatus.Success,
                Nonce = nonces.FirstOrDefault()
            };
        }








        private AcmeApiResponse ErrorResponse(string message)
        {
            return new AcmeApiResponse()
            {
                Status = AcmeApiResponseStatus.Error,
                Message = message
            };
        }

        private AcmeApiResponse<TData> ErrorResponse<TData>(string message)
        {
            return new AcmeApiResponse<TData>()
            {
                Status = AcmeApiResponseStatus.Error,
                Message = message
            };
        }
    }
}
