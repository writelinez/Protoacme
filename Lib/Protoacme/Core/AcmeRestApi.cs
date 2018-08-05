using Newtonsoft.Json;
using Protoacme.Core.Enumerations;
using Protoacme.Core.InternalModels;
using Protoacme.Core.Utilities;
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
    /// <summary>
    /// Low level Api used to interact with the ACME server.
    /// </summary>
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

        /// <summary>
        /// Gets the directory containing REST paths. used for future api requests.
        /// </summary>
        /// <returns>Directory containing REST paths. Wrapped by a response object.</returns>
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

        /// <summary>
        /// Get a new Nonce. Required for the majority of api requests.
        /// </summary>
        /// <param name="directory">Directory object.</param>
        /// <returns>Nonce string. Wrapped by a response object.</returns>
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

        /// <summary>
        /// Creates a new account.
        /// </summary>
        /// <param name="directory">Directory object.</param>
        /// <param name="nonce">Nonce</param>
        /// <param name="account">Information for new account.</param>
        /// <returns>Returns a serializable account object. Wrapped by a response object.</returns>
        /// <remarks>It is best to serialize and save the account object so it can be retrieved later and used for renewing domains.</remarks>
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

            var apiResp = await SendPostData(
                url: directory.NewAccount,
                data: jwsToken);

            string apiRespString = await apiResp.Content?.ReadAsStringAsync();
            if (apiResp.StatusCode != HttpStatusCode.Created)
                return ErrorResponse<AcmeAccount>(apiRespString);

            if (!apiResp.Headers.TryGetValues(ProtoacmeContants.HEADER_LOCATION, out IEnumerable<string> locations))
                return ErrorResponse<AcmeAccount>("Missing Location Header on CreateAccount Response.");

            if (!apiResp.Headers.TryGetValues(ProtoacmeContants.HEADER_NONCE, out IEnumerable<string> nonces))
                return ErrorResponse<AcmeAccount>("Missing Replay-Nonce Header on CreateAccount Response.");

            Dictionary<string, object> oResp = JsonConvert.DeserializeObject<Dictionary<string, object>>(apiRespString);

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

        /// <summary>
        /// Updates an existing accounts contacts
        /// </summary>
        /// <param name="directory">Directory object.</param>
        /// <param name="nonce">Nonce</param>
        /// <param name="account">Must be existing account.</param>
        /// <returns>Return api response with status.</returns>
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

            var apiResp = await SendPostData(
                url: _letsEncryptEndpoint.AppendUrl(ProtoacmeContants.LETSENCRYPT_ACCOUNT_FRAGMENT).AppendUrl(account.Id.ToString()),
                data: jwsToken);

            string apiRespString = await apiResp.Content?.ReadAsStringAsync();
            if (apiResp.StatusCode != HttpStatusCode.OK)
                return ErrorResponse(apiRespString);

            if (!apiResp.Headers.TryGetValues(ProtoacmeContants.HEADER_NONCE, out IEnumerable<string> nonces))
                return ErrorResponse<AcmeAccount>("Missing Replay-Nonce Header on CreateAccount (UPDATE) Response.");

            return new AcmeApiResponse()
            {
                Status = AcmeApiResponseStatus.Success,
                Nonce = nonces.FirstOrDefault()
            };
        }

        /// <summary>
        /// Changes and updates the account security info for an existing account.
        /// </summary>
        /// <param name="directory">Directory object.</param>
        /// <param name="nonce">Nonce</param>
        /// <param name="account">Must be existing account.</param>
        /// <returns>Return api response with status.</returns>
        /// <remarks>Will update the security info on the passed in account, so you will need to reserialize and update your existing account object to update the security info.</remarks>
        public async Task<AcmeApiResponse> RollOverAccountKey(AcmeDirectory directory, string nonce, AcmeAccount account)
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

            JwsContainer<ACCKEY> innerJwsObject = new JwsContainer<ACCKEY>(
                rsaPrams, 
                nonce, 
                directory.KeyChange, 
                new ACCKEY()
                {
                    account = account.KID,
                    newKey = new JWK()
                    {
                        e = Base64Tool.Encode(rsaPrams.Exponent),
                        kty = "RSA",
                        n = Base64Tool.Encode(rsaPrams.Modulus)
                    }
                });

            object signedInnerJwsObject = innerJwsObject.SerializeSignedObject();

            JwsContainer<object> outerJwsObject = new JwsContainer<object>(account.SecurityInfo, nonce, directory.KeyChange, account.KID, signedInnerJwsObject);

            string jwsToken = outerJwsObject.SerializeSignedToken();

            var apiResp = await SendPostData(
                url: directory.KeyChange,
                data: jwsToken);

            string apiRespString = await apiResp.Content?.ReadAsStringAsync();
            if (apiResp.StatusCode != HttpStatusCode.OK)
                return ErrorResponse(apiRespString);

            if (!apiResp.Headers.TryGetValues(ProtoacmeContants.HEADER_NONCE, out IEnumerable<string> nonces))
                return ErrorResponse<AcmeAccount>("Missing Replay-Nonce Header on RolloverKey Response.");

            account.SecurityInfo = rsaPrams;

            return new AcmeApiResponse()
            {
                Status = AcmeApiResponseStatus.Success,
                Nonce = nonces.FirstOrDefault()
            };
        }

        /// <summary>
        /// Deactivates an existing account.
        /// </summary>
        /// <param name="directory">Directory object.</param>
        /// <param name="nonce">Nonce</param>
        /// <param name="account">Must be existing account.</param>
        /// <returns>Return api response with status.</returns>
        /// <remarks>After this account has been deactivated it will no longer be valid in future requests.</remarks>
        public async Task<AcmeApiResponse> DeactivateAccount(AcmeDirectory directory, string nonce, AcmeAccount account)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (string.IsNullOrEmpty(directory.NewAccount))
                throw new ArgumentException("directory is missing Account url.");
            if (string.IsNullOrEmpty(nonce))
                throw new ArgumentNullException("nonce");
            if (account == null)
                throw new ArgumentNullException("account");

            JwsContainer<KILLACC> jwsObject = new JwsContainer<KILLACC>(account.SecurityInfo, nonce, account.KID, account.KID, new KILLACC() { status = "deactivated" });

            string jwsToken = jwsObject.SerializeSignedToken();

            var apiResp = await SendPostData(
                url: _letsEncryptEndpoint.AppendUrl(ProtoacmeContants.LETSENCRYPT_ACCOUNT_FRAGMENT).AppendUrl(account.Id.ToString()), 
                data: jwsToken);
            
            string apiRespString = await apiResp.Content?.ReadAsStringAsync();
            if (!apiResp.IsSuccessStatusCode)
                return ErrorResponse(apiRespString);

            if (!apiResp.Headers.TryGetValues(ProtoacmeContants.HEADER_NONCE, out IEnumerable<string> nonces))
                return ErrorResponse("Missing Replay-Nonce Header on DeactivateAccount Response.");

            return new AcmeApiResponse()
            {
                Status = AcmeApiResponseStatus.Success,
                Nonce = nonces.FirstOrDefault()
            };
        }

        /// <summary>
        /// Sends a new order requesting a new certificate.
        /// </summary>
        /// <param name="directory">Directory object.</param>
        /// <param name="nonce">Nonce</param>
        /// <param name="account">Must be existing account.</param>
        /// <param name="certificates">Info describing the dns entries you are requesting certificates for.</param>
        /// <returns>A certificate fulfillment promise that is used to complete the certification chain in future requests.  Wrapped by a response object.</returns>
        public async Task<AcmeApiResponse<AcmeCertificateFulfillmentPromise>> RequestCertificate(AcmeDirectory directory, string nonce, AcmeAccount account, AcmeCertificateRequest certificates)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (string.IsNullOrEmpty(directory.NewAccount))
                throw new ArgumentException("directory is missing Account url.");
            if (string.IsNullOrEmpty(nonce))
                throw new ArgumentNullException("nonce");
            if (account == null)
                throw new ArgumentNullException("account");
            if (certificates == null)
                throw new ArgumentNullException("certificates");
            if (certificates.Identifiers == null || !certificates.Identifiers.Any())
                throw new ArgumentException("Certificate is missing identifiers");

            JwsContainer<AcmeCertificateRequest> jwsObject = new JwsContainer<AcmeCertificateRequest>(account.SecurityInfo, nonce, directory.NewOrder, account.KID, certificates);

            string jwsToken = jwsObject.SerializeSignedToken();

            var apiResp = await SendPostData(
                url: directory.NewOrder,
                data: jwsToken);

            string apiRespString = await apiResp.Content?.ReadAsStringAsync();
            if (apiResp.StatusCode != HttpStatusCode.Created)
                return ErrorResponse<AcmeCertificateFulfillmentPromise>(apiRespString);

            if (!apiResp.Headers.TryGetValues(ProtoacmeContants.HEADER_NONCE, out IEnumerable<string> nonces))
                return ErrorResponse<AcmeCertificateFulfillmentPromise>("Missing Replay-Nonce Header on RequestCertificate Response.");

            return new AcmeApiResponse<AcmeCertificateFulfillmentPromise>()
            {
                Status = AcmeApiResponseStatus.Success,
                Nonce = nonces.FirstOrDefault(),
                Data = JsonConvert.DeserializeObject<AcmeCertificateFulfillmentPromise>(apiRespString)
            };
        }

        /// <summary>
        /// Gets challenges used to verify domain ownership.
        /// </summary>
        /// <param name="acmeCertificateFulfillmentPromise">The certificate fulfillment promise retrieved from the RequestCertificate call.</param>
        /// <returns>An authorization object containing the available challenge types. Wrapped by a response object.</returns>
        public async Task<List<AcmeApiResponse<AcmeAuthorization>>> GetChallenges(AcmeCertificateFulfillmentPromise acmeCertificateFulfillmentPromise)
        {
            List<AcmeApiResponse<AcmeAuthorization>> response = new List<AcmeApiResponse<AcmeAuthorization>>();

            if (acmeCertificateFulfillmentPromise == null)
                throw new ArgumentNullException("acmeCertificateFulfillmentPromise");
            if (acmeCertificateFulfillmentPromise.Authorizations == null || !acmeCertificateFulfillmentPromise.Authorizations.Any())
                throw new ArgumentException("No Authorizations exist in the Acme Certification Fulfillment Promise");

            foreach (string authUrl in acmeCertificateFulfillmentPromise.Authorizations)
            {
                AcmeApiResponse<AcmeAuthorization> result = new AcmeApiResponse<AcmeAuthorization>();

                var apiResp = await _httpClient.GetAsync(authUrl);
                string apiRespString = await apiResp.Content?.ReadAsStringAsync();
                if(!apiResp.IsSuccessStatusCode)
                {
                    result.Status = AcmeApiResponseStatus.Error;
                    result.Message = apiRespString;
                }
                else
                {
                    result.Status = AcmeApiResponseStatus.Success;
                    result.Data = JsonConvert.DeserializeObject<AcmeAuthorization>(apiRespString);
                }
                response.Add(result);
            }

            return response;
        }

        /// <summary>
        /// Start the challenge verification process.
        /// </summary>
        /// <param name="account">Must be existing account.</param>
        /// <param name="challenge">Single challenge from the AcmeAuthorization</param>
        /// <param name="nonce">Nonce</param>
        /// <param name="keyAuthorization">Authorization that identifies the domain and user.</param>
        /// <returns>The status of the challenge authorization. Wrapped by a response object.</returns>
        /// <remarks>This will need to be called on each domain that is used in the RequestCertificate call. You should not call this until the challenges are ready to verify. See https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.5 for more information.</remarks>
        public async Task<AcmeApiResponse<AcmeChallengeStatus>> VerifyChallenge(AcmeAccount account, AcmeChallenge challenge, string nonce, string keyAuthorization)
        {
            if (string.IsNullOrEmpty(nonce))
                throw new ArgumentNullException("nonce");
            if (account == null)
                throw new ArgumentNullException("account");
            if (challenge == null)
                throw new ArgumentNullException("challenge");
            if (string.IsNullOrEmpty(keyAuthorization))
                throw new ArgumentNullException("keyAuthorization");

            JwsContainer<KEYAUTH> jwsObject = new JwsContainer<KEYAUTH>(account.SecurityInfo, nonce, challenge.Url, account.KID, new KEYAUTH() { keyAuthorization = keyAuthorization });

            string jwsToken = jwsObject.SerializeSignedToken();

            var apiResp = await SendPostData(
                url: challenge.Url,
                data: jwsToken);

            string apiRespString = await apiResp.Content?.ReadAsStringAsync();
            if (apiResp.StatusCode != HttpStatusCode.OK)
                return ErrorResponse<AcmeChallengeStatus>(apiRespString);

            if (!apiResp.Headers.TryGetValues(ProtoacmeContants.HEADER_NONCE, out IEnumerable<string> nonces))
                return ErrorResponse<AcmeChallengeStatus>("Missing Replay-Nonce Header on CompleteChallenge Response.");

            return new AcmeApiResponse<AcmeChallengeStatus>()
            {
                Status = AcmeApiResponseStatus.Success,
                Nonce = nonces.FirstOrDefault(),
                Data = JsonConvert.DeserializeObject<AcmeChallengeStatus>(apiRespString)
            };
        }

        /// <summary>
        /// Gets the status of the challenge verification.
        /// </summary>
        /// <param name="challenge">Single challenge from the AcmeAuthorization</param>
        /// <returns>Returns the status of the challenge. Wrapped by a response object.</returns>
        public async Task<AcmeApiResponse<AcmeChallengeVerificationStatus>> GetChallengeVerificationStatus(AcmeChallenge challenge)
        {
            if (challenge == null)
                throw new ArgumentNullException("challenge");

            var apiResp = await _httpClient.GetAsync(challenge.Url);

            string apiRespString = await apiResp.Content?.ReadAsStringAsync();
            if (apiResp.StatusCode != HttpStatusCode.OK)
                return ErrorResponse<AcmeChallengeVerificationStatus>(apiRespString);

            return new AcmeApiResponse<AcmeChallengeVerificationStatus>()
            {
                Status = AcmeApiResponseStatus.Success,
                Data = JsonConvert.DeserializeObject<AcmeChallengeVerificationStatus>(apiRespString)
            };
        }

        /// <summary>
        /// Finalize certificate request. This allows the certificate to be downloaded by using the GetCertificate request.
        /// </summary>
        /// <param name="account">Must be existing account.</param>
        /// <param name="nonce">Nonce</param>
        /// <param name="acmeCertificateFulfillmentPromise">The original Certificate Fulfillment Promise used in the RequestCertificate request.</param>
        /// <param name="csr">The certificate CSR. This can be generated using helpers through this api or by an external source such as IIS.</param>
        /// <returns>A completed Certificate Fulfillment Promise used to Download the certificate using the GetCertificate call. Wrapped by a response object.</returns>
        public async Task<AcmeApiResponse<AcmeCertificateFulfillmentPromise>> FinalizeChallenge(AcmeAccount account, string nonce, AcmeCertificateFulfillmentPromise acmeCertificateFulfillmentPromise, string csr)
        {
            if (string.IsNullOrEmpty(nonce))
                throw new ArgumentNullException("nonce");
            if (account == null)
                throw new ArgumentNullException("account");
            if (acmeCertificateFulfillmentPromise == null)
                throw new ArgumentNullException("acmeCertificateFulfillmentPromise");
            if (string.IsNullOrEmpty(csr))
                throw new ArgumentNullException("csr");

            JwsContainer<CSR> jwsObject = new JwsContainer<CSR>(account.SecurityInfo, nonce, acmeCertificateFulfillmentPromise.Finalize, account.KID, new CSR() { csr = csr });

            string jwsToken = jwsObject.SerializeSignedToken();

            var apiResp = await SendPostData(
                url: acmeCertificateFulfillmentPromise.Finalize,
                data: jwsToken);

            string apiRespString = await apiResp.Content?.ReadAsStringAsync();
            if (apiResp.StatusCode != HttpStatusCode.OK)
                return ErrorResponse<AcmeCertificateFulfillmentPromise>(apiRespString);

            if (!apiResp.Headers.TryGetValues(ProtoacmeContants.HEADER_NONCE, out IEnumerable<string> nonces))
                return ErrorResponse<AcmeCertificateFulfillmentPromise>("Missing Replay-Nonce Header on FinalizeChallenge Response.");

            return new AcmeApiResponse<AcmeCertificateFulfillmentPromise>()
            {
                Status = AcmeApiResponseStatus.Success,
                Nonce = nonces.FirstOrDefault(),
                Data = JsonConvert.DeserializeObject<AcmeCertificateFulfillmentPromise>(apiRespString)
            };
        }

        /// <summary>
        /// Downloads the SSL Certificate.
        /// </summary>
        /// <param name="completedPromise">The completed certificate fulfillment promise retreived from the FinalizeChallenge call.</param>
        /// <param name="certificateType">The type of certificate you are requesting.</param>
        /// <returns>The certificate.</returns>
        public async Task<AcmeApiResponse<ArraySegment<byte>>> GetCertificate(AcmeCertificateFulfillmentPromise completedPromise, CertificateType certificateType)
        {
            if (completedPromise == null)
                throw new ArgumentNullException("completedPromise");
            if (string.IsNullOrEmpty(completedPromise.Certificate))
                throw new ArgumentException("Certificate url is not valid");
            if (certificateType == null)
                throw new ArgumentNullException("certificateType");

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, completedPromise.Certificate);
            message.Headers.Accept.ParseAdd($"application/{certificateType.Value}");
            var apiResp = await _httpClient.SendAsync(message);

            string apiRespString = await apiResp.Content?.ReadAsStringAsync();
            if (apiResp.StatusCode != HttpStatusCode.OK)
                return ErrorResponse<ArraySegment<byte>>(apiRespString);

            byte[] bContent = await apiResp.Content.ReadAsByteArrayAsync();

            return new AcmeApiResponse<ArraySegment<byte>>()
            {
                Status = AcmeApiResponseStatus.Success,
                Data = new ArraySegment<byte>(bContent)
            };
        }

        #region Private Functions
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

        private async Task<HttpResponseMessage> SendPostData(string url, string data)
        {
            StringContent content = new StringContent(data);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/jose+json");

            HttpResponseMessage resp = await _httpClient.PostAsync(url, content);
            return resp;
        }
        #endregion
    }
}
