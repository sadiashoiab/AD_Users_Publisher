﻿using System;
using System.Threading.Tasks;
using Azure_AD_Users_Extract.Services;
using Azure_AD_Users_Shared.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Azure_AD_Users_Extract.Tests
{
    [TestClass]
    public class TokenServiceTests
    {
        [TestMethod]
        public async Task RetrieveFranchiseToken()
        {
            // ARRANGE
            var keyVaultServiceMock = new Mock<IAzureKeyVaultService>();
            var tokenProvider = new Mock<IGraphApiService>();
            var unitUnderTest = new TokenService(keyVaultServiceMock.Object, tokenProvider.Object);

            var clientId = "CLIENTID";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("NAClientId"))
                .ReturnsAsync(() => clientId).Verifiable();
            
            var appKey = "APPKEY";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("NAAppKey"))
                .ReturnsAsync(() => appKey).Verifiable();

            var tenant = "TENANT";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("NATenant"))
                .ReturnsAsync(() => tenant).Verifiable();

            var expectedToken = "TOKEN";
            tokenProvider.Setup(mock => mock.AcquireToken(clientId, appKey, tenant))
                .ReturnsAsync(() => expectedToken).Verifiable();

            // ACT
            var result = await unitUnderTest.RetrieveToken(TokenEnum.Franchise);

            // ASSERT
            keyVaultServiceMock.Verify();
            Assert.AreEqual(expectedToken, result);
        }

        [TestMethod]
        public async Task RetrieveHomeOfficeToken()
        {
            // ARRANGE
            var keyVaultServiceMock = new Mock<IAzureKeyVaultService>();
            var tokenProvider = new Mock<IGraphApiService>();
            var unitUnderTest = new TokenService(keyVaultServiceMock.Object, tokenProvider.Object);

            var clientId = "CLIENTID";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("HOClientId"))
                .ReturnsAsync(() => clientId).Verifiable();
            
            var appKey = "APPKEY";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("HOAppKey"))
                .ReturnsAsync(() => appKey).Verifiable();

            var tenant = "TENANT";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("HOTenant"))
                .ReturnsAsync(() => tenant).Verifiable();

            var expectedToken = "TOKEN";
            tokenProvider.Setup(mock => mock.AcquireToken(clientId, appKey, tenant))
                .ReturnsAsync(() => expectedToken).Verifiable();

            // ACT
            var result = await unitUnderTest.RetrieveToken(TokenEnum.HomeOffice);

            // ASSERT
            keyVaultServiceMock.Verify();
            Assert.AreEqual(expectedToken, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task Retrieve_ArgumentOutOfRangeException()
        {
            // ARRANGE
            var keyVaultServiceMock = new Mock<IAzureKeyVaultService>();
            var tokenProvider = new Mock<IGraphApiService>();
            var unitUnderTest = new TokenService(keyVaultServiceMock.Object, tokenProvider.Object);

            // ACT
            var _ = await unitUnderTest.RetrieveToken(TokenEnum.Invalid);
        }
    }
}
