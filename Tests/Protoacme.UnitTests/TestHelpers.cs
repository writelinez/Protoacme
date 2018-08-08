using Protoacme.Core.Enumerations;
using Protoacme.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Protoacme.UnitTests
{
    public static class TestHelpers
    {
        public static AcmeApiResponse<AcmeDirectory> AcmeDirectoryResponse
        {
            get
            {
                return new AcmeApiResponse<AcmeDirectory>()
                {
                    Status = AcmeApiResponseStatus.Success,
                    Nonce = Guid.NewGuid().ToString(),
                    Data = new AcmeDirectory()
                    {
                        KeyChange = Guid.NewGuid().ToString(),
                        Meta = new AcmeMeta(),
                        NewAccount = Guid.NewGuid().ToString(),
                        NewNoonce = Guid.NewGuid().ToString(),
                        NewOrder = Guid.NewGuid().ToString(),
                        RevokeCert = Guid.NewGuid().ToString()
                    }
                };
            }
        }

        public static AcmeApiResponse AcmeEmptyResponseWithNonce
        {
            get
            {
                return new AcmeApiResponse()
                {
                    Status = AcmeApiResponseStatus.Success,
                    Nonce = Guid.NewGuid().ToString()
                };
            }
        }

        public static AcmeApiResponse<AcmeAccount> AcmeAccountResponse
        {
            get
            {
                RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
                RSAParameters rsaParams = rsaProvider.ExportParameters(true);

                Random ran = new Random();
                int num = ran.Next(1, 1000);

                return new AcmeApiResponse<AcmeAccount>()
                {
                    Status = AcmeApiResponseStatus.Success,
                    Nonce = Guid.NewGuid().ToString(),
                    Data = new AcmeAccount()
                    {
                        Contact = new List<string>()
                    {
                        Guid.NewGuid().ToString()
                    },
                        Id = num,
                        KID = Guid.NewGuid().ToString(),
                        SecurityInfo = rsaParams
                    }
                };
            }
        }

        public static AcmeCreateAccount CreateAccount
        {
            get
            {
                return new AcmeCreateAccount()
                {
                    Contact = new List<string>()
                    {
                        Guid.NewGuid().ToString()
                    },
                    TermsOfServiceAgreed = true
                };
            }
        }
    }
}
