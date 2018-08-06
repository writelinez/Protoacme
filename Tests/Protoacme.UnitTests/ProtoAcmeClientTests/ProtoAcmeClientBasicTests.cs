using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Protoacme.Core.Abstractions;
using Protoacme.Core.Enumerations;
using Protoacme.Models;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Protoacme.UnitTests.ProtoAcmeClientTests
{
    [TestClass]
    public class ProtoAcmeClientBasicTests
    {
        [TestMethod]
        public async Task CreateAccount_ShouldReturnNewAccount()
        {
            //SETUP
            Mock<IAcmeRestApi> restApiMock = new Mock<IAcmeRestApi>();
            ProtoAcmeClient client = new ProtoAcmeClient(restApiMock.Object);

            AcmeApiResponse<AcmeDirectory> directoryResponse = CreateDirectoryResponse();
            AcmeApiResponse nonceResponse = CreateNonceResponse();
            AcmeApiResponse<AcmeAccount> accountResponse = CreateAccountResponse();

            restApiMock.Setup(method => method.GetDirectoryAsync())
                .ReturnsAsync(directoryResponse);

            restApiMock.Setup(method => method.GetNonceAsync(It.IsAny<AcmeDirectory>()))
                .ReturnsAsync(nonceResponse);

            restApiMock.Setup(method => method.CreateAccountAsync(It.IsAny<AcmeDirectory>(), It.IsAny<string>(), It.IsAny<AcmeCreateAccount>()))
                .ReturnsAsync(accountResponse);

            //EXECUTE
            var expectedAccountResponse = await client.CreateAccountAsync(new AcmeCreateAccount() { Contact = new List<string>() { "a" }, TermsOfServiceAgreed = true });

            //ASSERT
            restApiMock.Verify(method => method.GetDirectoryAsync(), Times.Once());
            restApiMock.Verify(method => method.GetNonceAsync(directoryResponse.Data), Times.Once());
            expectedAccountResponse.ShouldNotBeNull();
            expectedAccountResponse.Contact.Count.ShouldBe(1);
        }






        private AcmeApiResponse<AcmeDirectory> CreateDirectoryResponse
            (
            AcmeApiResponseStatus status = AcmeApiResponseStatus.Success,
            string nonce = "abc"
            )
        {
            return new AcmeApiResponse<AcmeDirectory>()
            {
                Status = status,
                Nonce = nonce,
                Data = new AcmeDirectory()
                {
                    KeyChange = "a",
                    Meta = new AcmeMeta(),
                    NewAccount = "b",
                    NewNoonce = "f",
                    NewOrder = "c",
                    RevokeCert = "d"
                }
            };
        }

        private AcmeApiResponse CreateNonceResponse
            (
            AcmeApiResponseStatus status = AcmeApiResponseStatus.Success,
            string nonce = "abc"
            )
        {
            return new AcmeApiResponse()
            {
                Status = status,
                Nonce = nonce
            };
        }

        private AcmeApiResponse<AcmeAccount> CreateAccountResponse
            (
            AcmeApiResponseStatus status = AcmeApiResponseStatus.Success,
            string nonce = "abc"
            )
        {
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            RSAParameters rsaParams = rsaProvider.ExportParameters(true);

            return new AcmeApiResponse<AcmeAccount>()
            {
                Status = status,
                Nonce = nonce,
                Data = new AcmeAccount()
                {
                    Contact = new List<string>()
                    {
                        "c"
                    },
                    Id = 1,
                    KID = "c",
                    SecurityInfo = rsaParams
                }
            };
        }
    }
}
