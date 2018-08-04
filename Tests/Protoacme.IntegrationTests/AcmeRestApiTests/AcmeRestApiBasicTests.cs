using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protoacme.Core;
using Protoacme.Core.Enumerations;
using Protoacme.Models;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Protoacme.IntegrationTests.AcmeRestApiTests
{
    [TestClass]
    public class AcmeRestApiBasicTests
    {
        [TestMethod]
        public async Task GetDirectory_ShouldReturnValidDirectoryInfo()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directoryResponse;

            //EXECUTE
            directoryResponse = await api.GetDirectoryAsync();

            //ASSERT
            directoryResponse.Status.ShouldBe(AcmeApiResponseStatus.Success);
            directoryResponse.Data.ShouldNotBeNull();
            directoryResponse.Data.NewNoonce.ShouldNotBeNull();
            directoryResponse.Data.NewNoonce.Length.ShouldBeGreaterThan(0);
            directoryResponse.Data.NewAccount.ShouldNotBeNull();
            directoryResponse.Data.NewAccount.Length.ShouldBeGreaterThan(0);
        }

        [TestMethod]
        public async Task GetNewNonce_ShouldReturnNewReplayNonce()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directoryResponse;
            AcmeApiResponse nonceResponse = null;

            //EXECUTE
            directoryResponse = await api.GetDirectoryAsync();
            nonceResponse = await api.GetNonce(directoryResponse.Data);

            //ASSERT
            nonceResponse.ShouldNotBeNull();
            nonceResponse.Status.ShouldBe(AcmeApiResponseStatus.Success);
            nonceResponse.Nonce.Length.ShouldBeGreaterThan(0);
        }

        [TestMethod]
        public async Task CreateAccount_ShouldSuccessfullyCreateNewAccount()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directory;
            AcmeApiResponse nonceResponse = null;
            AcmeApiResponse<AcmeAccount> accountResponse = null;

            //EXECUTE
            directory = await api.GetDirectoryAsync();
            nonceResponse = await api.GetNonce(directory.Data);
            accountResponse = await api.CreateAccount(directory.Data, nonceResponse.Nonce, new AcmeCreateAccount() { Contact = new List<string>() { "mailto:bob@toast.com" }, TermsOfServiceAgreed = true });

            //ASSERT
            accountResponse.ShouldNotBeNull();
            accountResponse.Status.ShouldBe(AcmeApiResponseStatus.Success);
            accountResponse.Data.KID.ShouldNotBeNull();
            accountResponse.Data.KID.Length.ShouldBeGreaterThan(0);
            accountResponse.Data.Contact.ShouldNotBeNull();
            accountResponse.Data.Contact.Count.ShouldBe(1);
            accountResponse.Data.SecurityInfo.ShouldNotBeNull();
        }

        [TestMethod]
        public async Task UpdateAccount_ShoudUpdateSuccessfully()
        {
            //SETUP
            AcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
            AcmeApiResponse<AcmeDirectory> directory;
            AcmeApiResponse nonceResponse = null;
            AcmeApiResponse<AcmeAccount> accountResponse = null;
            AcmeApiResponse updateAccountResponse = null;

            //EXECUTE
            directory = await api.GetDirectoryAsync();
            nonceResponse = await api.GetNonce(directory.Data);
            accountResponse = await api.CreateAccount(directory.Data, nonceResponse.Nonce, new AcmeCreateAccount() { Contact = new List<string>() { "mailto:bob@toast.com" }, TermsOfServiceAgreed = true });
            updateAccountResponse = await api.UpdateAccount(directory.Data, accountResponse.Nonce, accountResponse.Data);

            //ASSERT
            updateAccountResponse.ShouldNotBeNull();
            updateAccountResponse.Status.ShouldBe(AcmeApiResponseStatus.Success);
            updateAccountResponse.Nonce.Length.ShouldBeGreaterThan(0);
        }
    }
}
