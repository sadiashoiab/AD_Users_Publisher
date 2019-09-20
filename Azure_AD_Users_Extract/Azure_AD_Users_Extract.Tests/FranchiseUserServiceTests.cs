using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure_AD_Users_Extract.Services;
using Azure_AD_Users_Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Azure_AD_Users_Extract.Tests
{
    [TestClass]
    public class FranchiseUserServiceTests
    {
        [TestMethod]
        public async Task GetFranchiseUsersWithSuccessfulResponseAndNoData()
        {
            // ARRANGE
            var loggerMock = new Mock<ILogger<FranchiseUserService>>();
            var tokenServiceMock = new Mock<ITokenService>();
            var userServiceMock = new Mock<IUserService>();
            var unitUnderTest = new FranchiseUserService(loggerMock.Object, tokenServiceMock.Object, userServiceMock.Object);
            var groupId = "GROUP";
            var syncDuration = 2;

            var token = "TOKEN";

            tokenServiceMock.Setup(mock => mock.RetrieveToken(TokenEnum.Franchise))
                .ReturnsAsync(() => token).Verifiable();
            userServiceMock.Setup(mock => mock.GetUsers(groupId, token, syncDuration))
                .ReturnsAsync(() => new List<AzureActiveDirectoryUser>()).Verifiable();

            // ACT
            var results = await unitUnderTest.GetFranchiseUsers(groupId, syncDuration);

            // ASSERT
            tokenServiceMock.Verify();
            userServiceMock.Verify();
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task GetFranchiseUsersWithSuccessfulResponseAndData()
        {
            // ARRANGE
            var loggerMock = new Mock<ILogger<FranchiseUserService>>();
            var tokenServiceMock = new Mock<ITokenService>();
            var userServiceMock = new Mock<IUserService>();
            var unitUnderTest = new FranchiseUserService(loggerMock.Object, tokenServiceMock.Object, userServiceMock.Object);
            var groupId = "GROUP";
            var syncDuration = 2;

            var token = "TOKEN";

            tokenServiceMock.Setup(mock => mock.RetrieveToken(TokenEnum.Franchise))
                .ReturnsAsync(() => token).Verifiable();
            userServiceMock.Setup(mock => mock.GetUsers(groupId, token, syncDuration))
                .ReturnsAsync(() => new List<AzureActiveDirectoryUser> {new AzureActiveDirectoryUser()}).Verifiable();

            // ACT
            var results = await unitUnderTest.GetFranchiseUsers(groupId, syncDuration);

            // ASSERT
            tokenServiceMock.Verify();
            userServiceMock.Verify();
            loggerMock.Verify(mock => mock.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(v => v.ToString().Contains("1 Franchise users were retrieved")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task GetFranchiseUsersWithException()
        {
            // ARRANGE
            var loggerMock = new Mock<ILogger<FranchiseUserService>>();
            var tokenServiceMock = new Mock<ITokenService>();
            var userServiceMock = new Mock<IUserService>();
            var unitUnderTest = new FranchiseUserService(loggerMock.Object, tokenServiceMock.Object, userServiceMock.Object);
            var groupId = "GROUP";
            var syncDuration = 2;

            var token = "TOKEN";

            tokenServiceMock.Setup(mock => mock.RetrieveToken(TokenEnum.Franchise)).Throws(new Exception("TEST EXCEPTION")).Verifiable();

            // ACT
            var results = await unitUnderTest.GetFranchiseUsers(groupId, syncDuration);

            // ASSERT
            tokenServiceMock.Verify();
            loggerMock.Verify(mock => mock.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(v => v.ToString().StartsWith("Error occurred while trying to get token and users for groupId:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
            Assert.IsNull(results);
        }

        [TestMethod]
        public async Task GetFranchiseDeactivatedUsersWithSuccessfulResponseAndNoData()
        {
            // ARRANGE
            var loggerMock = new Mock<ILogger<FranchiseUserService>>();
            var tokenServiceMock = new Mock<ITokenService>();
            var userServiceMock = new Mock<IUserService>();
            var unitUnderTest = new FranchiseUserService(loggerMock.Object, tokenServiceMock.Object, userServiceMock.Object);
            var groupId = "GROUP";
            var syncDuration = 2;

            var token = "TOKEN";

            tokenServiceMock.Setup(mock => mock.RetrieveToken(TokenEnum.Franchise))
                .ReturnsAsync(() => token).Verifiable();
            userServiceMock.Setup(mock => mock.GetDeactivatedUsers(token, syncDuration))
                .ReturnsAsync(() => new List<AzureActiveDirectoryUser>()).Verifiable();

            // ACT
            var results = await unitUnderTest.GetFranchiseDeactivatedUsers(syncDuration);

            // ASSERT
            tokenServiceMock.Verify();
            userServiceMock.Verify();
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task GetFranchiseDeactivatedUsersWithSuccessfulResponseAndData()
        {
            // ARRANGE
            var loggerMock = new Mock<ILogger<FranchiseUserService>>();
            var tokenServiceMock = new Mock<ITokenService>();
            var userServiceMock = new Mock<IUserService>();
            var unitUnderTest = new FranchiseUserService(loggerMock.Object, tokenServiceMock.Object, userServiceMock.Object);
            var groupId = "GROUP";
            var syncDuration = 2;

            var token = "TOKEN";

            tokenServiceMock.Setup(mock => mock.RetrieveToken(TokenEnum.Franchise))
                .ReturnsAsync(() => token).Verifiable();
            userServiceMock.Setup(mock => mock.GetDeactivatedUsers(token, syncDuration))
                .ReturnsAsync(() => new List<AzureActiveDirectoryUser> {new AzureActiveDirectoryUser()}).Verifiable();

            // ACT
            var results = await unitUnderTest.GetFranchiseDeactivatedUsers(syncDuration);

            // ASSERT
            tokenServiceMock.Verify();
            userServiceMock.Verify();
            loggerMock.Verify(mock => mock.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(v => v.ToString().Contains("1 Deactivated Franchise users were retrieved")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task GetFranchiseDeactivatedUsersWithException()
        {
            // ARRANGE
            var loggerMock = new Mock<ILogger<FranchiseUserService>>();
            var tokenServiceMock = new Mock<ITokenService>();
            var userServiceMock = new Mock<IUserService>();
            var unitUnderTest = new FranchiseUserService(loggerMock.Object, tokenServiceMock.Object, userServiceMock.Object);
            var syncDuration = 2;

            var token = "TOKEN";

            tokenServiceMock.Setup(mock => mock.RetrieveToken(TokenEnum.Franchise)).Throws(new Exception("TEST EXCEPTION")).Verifiable();

            // ACT
            var results = await unitUnderTest.GetFranchiseDeactivatedUsers(syncDuration);

            // ASSERT
            tokenServiceMock.Verify();
            loggerMock.Verify(mock => mock.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(v => v.ToString().Equals($"error occurred while trying to get token and deactivated users with syncDurationInHours: {syncDuration}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
            Assert.IsNull(results);
        }
    }
}