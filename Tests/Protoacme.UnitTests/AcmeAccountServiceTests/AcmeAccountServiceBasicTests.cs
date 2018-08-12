using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Protoacme.Core.Abstractions;
using Protoacme.Models;
using Protoacme.Services;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Protoacme.UnitTests.AcmeAccountServiceTests
{
    [TestClass]
    public class AcmeAccountServiceBasicTests
    {
        [TestMethod]
        public async Task CreateAccount_ShouldUpdateLastNonce()
        {
            //ARRANGE
            var acmeApiMock = new Mock<IAcmeRestApi>();
            var directoryCacheMock = new Mock<ICachedRepository<AcmeDirectory>>();
            var nonceCacheMock = new Mock<ICachedRepository<string>>();

            AcmeCreateAccount inputAccount = TestHelpers.CreateAccount;
            AcmeApiResponse<AcmeAccount> accountResponse = TestHelpers.AcmeAccountResponse;

            acmeApiMock.Setup(method => method.CreateAccountAsync(It.IsAny<AcmeDirectory>(), It.IsAny<string>(), It.IsAny<AcmeCreateAccount>()))
                .ReturnsAsync(accountResponse);

            AcmeAccountService srv = new AcmeAccountService(acmeApiMock.Object, directoryCacheMock.Object, nonceCacheMock.Object);

            //ACT
            await srv.CreateAsync(inputAccount);

            //ASSERT
            nonceCacheMock.Verify(method => method.Update(accountResponse.Nonce), Times.Once());
        }

        [TestMethod]
        public async Task CreateAccount_ShouldReturnExpectedNewAccount()
        {
            //ARRANGE
            var acmeApiMock = new Mock<IAcmeRestApi>();
            var directoryCacheMock = new Mock<ICachedRepository<AcmeDirectory>>();
            var nonceCacheMock = new Mock<ICachedRepository<string>>();

            AcmeCreateAccount inputAccount = TestHelpers.CreateAccount;
            AcmeApiResponse<AcmeAccount> accountResponse = TestHelpers.AcmeAccountResponse;

            acmeApiMock.Setup(method => method.CreateAccountAsync(It.IsAny<AcmeDirectory>(), It.IsAny<string>(), It.IsAny<AcmeCreateAccount>()))
                .ReturnsAsync(accountResponse);

            AcmeAccountService srv = new AcmeAccountService(acmeApiMock.Object, directoryCacheMock.Object, nonceCacheMock.Object);

            //ACT
            var expected = await srv.CreateAsync(inputAccount);

            //ASSERT
            expected.ShouldBe(accountResponse.Data);
        }

        [TestMethod]
        public async Task UpdateAccount_ShouldUpdateLastNonce()
        {
            //ARRANGE
            var acmeApiMock = new Mock<IAcmeRestApi>();
            var directoryCacheMock = new Mock<ICachedRepository<AcmeDirectory>>();
            var nonceCacheMock = new Mock<ICachedRepository<string>>();

            AcmeApiResponse successResponse = TestHelpers.AcmeEmptyResponseWithNonce;
            AcmeAccount account = TestHelpers.AcmeAccountResponse.Data;

            acmeApiMock.Setup(method => method.UpdateAccountAsync(It.IsAny<AcmeDirectory>(), It.IsAny<string>(), It.IsAny<AcmeAccount>()))
                .ReturnsAsync(successResponse);

            AcmeAccountService srv = new AcmeAccountService(acmeApiMock.Object, directoryCacheMock.Object, nonceCacheMock.Object);

            //ACT
            await srv.UpdateAsync(account);

            //ASSERT
            nonceCacheMock.Verify(method => method.Update(successResponse.Nonce), Times.Once());
        }

        [TestMethod]
        public async Task ChangeKey_ShouldUpdateLastNonce()
        {
            //ARRANGE
            var acmeApiMock = new Mock<IAcmeRestApi>();
            var directoryCacheMock = new Mock<ICachedRepository<AcmeDirectory>>();
            var nonceCacheMock = new Mock<ICachedRepository<string>>();

            AcmeApiResponse successResponse = TestHelpers.AcmeEmptyResponseWithNonce;
            AcmeAccount account = TestHelpers.AcmeAccountResponse.Data;

            acmeApiMock.Setup(method => method.RollOverAccountKeyAsync(It.IsAny<AcmeDirectory>(), It.IsAny<string>(), It.IsAny<AcmeAccount>()))
                .ReturnsAsync(successResponse);

            AcmeAccountService srv = new AcmeAccountService(acmeApiMock.Object, directoryCacheMock.Object, nonceCacheMock.Object);

            //ACT
            await srv.ChangeKeyAsync(account);

            //ASSERT
            nonceCacheMock.Verify(method => method.Update(successResponse.Nonce), Times.Once());
        }

        [TestMethod]
        public async Task Deactivate_ShouldUpdateLastNonce()
        {
            //ARRANGE
            var acmeApiMock = new Mock<IAcmeRestApi>();
            var directoryCacheMock = new Mock<ICachedRepository<AcmeDirectory>>();
            var nonceCacheMock = new Mock<ICachedRepository<string>>();

            AcmeApiResponse successResponse = TestHelpers.AcmeEmptyResponseWithNonce;
            AcmeAccount account = TestHelpers.AcmeAccountResponse.Data;

            acmeApiMock.Setup(method => method.DeactivateAccountAsync(It.IsAny<AcmeDirectory>(), It.IsAny<string>(), It.IsAny<AcmeAccount>()))
                .ReturnsAsync(successResponse);

            AcmeAccountService srv = new AcmeAccountService(acmeApiMock.Object, directoryCacheMock.Object, nonceCacheMock.Object);

            //ACT
            await srv.DeactiveAsync(account);

            //ASSERT
            nonceCacheMock.Verify(method => method.Update(successResponse.Nonce), Times.Once());
        }
    }
}
