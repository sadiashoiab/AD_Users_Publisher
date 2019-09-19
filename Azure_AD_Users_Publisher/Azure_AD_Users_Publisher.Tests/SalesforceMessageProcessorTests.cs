using System;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services;
using Azure_AD_Users_Publisher.Services.Models;
using Azure_AD_Users_Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Azure_AD_Users_Publisher.Tests
{
    [TestClass]
    public class SalesforceMessageProcessorTests
    {
        [TestMethod]
        public async Task ProcessMessage_SuccessSyncUserWithNonClearCareOperatingSystem()
        {
            // ARRANGE
            var loggerMock = new Mock<ILogger<SalesforceMessageProcessor>>();
            var tokenServiceMock = new Mock<IHISCTokenService>();
            var programDataServiceMock = new Mock<IProgramDataService>();
            var timeZoneServiceMock = new Mock<ITimeZoneService>();
            var salesforceUserPublishServiceMock = new Mock<ISalesforceUserPublishService>();

            var unitUnderTest = new SalesforceMessageProcessor(loggerMock.Object,
                tokenServiceMock.Object,
                programDataServiceMock.Object,
                timeZoneServiceMock.Object,
                salesforceUserPublishServiceMock.Object);

            var salesforceFranchises = new[]
            {
                100
            };
            var clearCareFranchises = new[]
            {
                200
            };

            var token = "TOKEN";
            tokenServiceMock.Setup(mock => mock.RetrieveToken())
                .ReturnsAsync(token)
                .Verifiable();
            programDataServiceMock.Setup(mock => mock.RetrieveFranchises(ProgramDataSources.Salesforce, token, true))
                .ReturnsAsync(salesforceFranchises)
                .Verifiable();
            programDataServiceMock.Setup(mock => mock.RetrieveFranchises(ProgramDataSources.ClearCare, token, true))
                .ReturnsAsync(clearCareFranchises)
                .Verifiable();

            var timeZone = "Americas/NewYork";
            timeZoneServiceMock.Setup(mock => mock.RetrieveTimeZoneAndPopulateUsersCountryCode(It.IsAny<AzureActiveDirectoryUser>()))
                .ReturnsAsync(timeZone)
                .Verifiable();

            var salesforceUser = new AzureActiveDirectoryUser
            {
                Address = "11218 John Galt Blvd.",
                FranchiseNumber = "100"
            };

            // ACT
            await unitUnderTest.ProcessUser(salesforceUser);

            // ASSERT
            tokenServiceMock.Verify();
            programDataServiceMock.Verify();
            loggerMock.Verify(mock => mock.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(v => v.ToString().Contains("will be Published")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
            timeZoneServiceMock.Verify();
            // todo: put back after testing
            //salesforceUserPublishServiceMock.Verify(mock => mock.Publish(It.IsAny<SalesforceUser>()));
        }

        [TestMethod]
        public async Task ProcessMessage_SuccessSyncUserWithClearCareOperatingSystem()
        {
            // ARRANGE
            var loggerMock = new Mock<ILogger<SalesforceMessageProcessor>>();
            var tokenServiceMock = new Mock<IHISCTokenService>();
            var programDataServiceMock = new Mock<IProgramDataService>();
            var timeZoneServiceMock = new Mock<ITimeZoneService>();
            var salesforceUserPublishServiceMock = new Mock<ISalesforceUserPublishService>();

            var unitUnderTest = new SalesforceMessageProcessor(loggerMock.Object,
                tokenServiceMock.Object,
                programDataServiceMock.Object,
                timeZoneServiceMock.Object,
                salesforceUserPublishServiceMock.Object);

            var salesforceFranchises = new[]
            {
                100
            };
            var clearCareFranchises = new[]
            {
                100
            };

            var token = "TOKEN";
            tokenServiceMock.Setup(mock => mock.RetrieveToken())
                .ReturnsAsync(token)
                .Verifiable();
            programDataServiceMock.Setup(mock => mock.RetrieveFranchises(ProgramDataSources.Salesforce, token, true))
                .ReturnsAsync(salesforceFranchises)
                .Verifiable();
            programDataServiceMock.Setup(mock => mock.RetrieveFranchises(ProgramDataSources.ClearCare, token, true))
                .ReturnsAsync(clearCareFranchises)
                .Verifiable();

            var timeZone = "Americas/NewYork";
            timeZoneServiceMock.Setup(mock => mock.RetrieveTimeZoneAndPopulateUsersCountryCode(It.IsAny<AzureActiveDirectoryUser>()))
                .ReturnsAsync(timeZone)
                .Verifiable();

            var salesforceUser = new AzureActiveDirectoryUser
            {
                Address = "11218 John Galt Blvd.",
                FranchiseNumber = "100"
            };

            // ACT
            await unitUnderTest.ProcessUser(salesforceUser);

            // ASSERT
            tokenServiceMock.Verify();
            programDataServiceMock.Verify();
            loggerMock.Verify(mock => mock.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(v => v.ToString().Contains("will be Published")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
            timeZoneServiceMock.Verify();
            // todo: put back after testing
            //salesforceUserPublishServiceMock.Verify(mock => mock.Publish(It.IsAny<SalesforceUser>()));
        }

        [TestMethod]
        public async Task ProcessMessage_SuccessAndDoNotSyncUser()
        {
            // ARRANGE
            var loggerMock = new Mock<ILogger<SalesforceMessageProcessor>>();
            var tokenServiceMock = new Mock<IHISCTokenService>();
            var programDataServiceMock = new Mock<IProgramDataService>();
            var timeZoneServiceMock = new Mock<ITimeZoneService>();
            var salesforceUserPublishServiceMock = new Mock<ISalesforceUserPublishService>();

            var unitUnderTest = new SalesforceMessageProcessor(loggerMock.Object,
                tokenServiceMock.Object,
                programDataServiceMock.Object,
                timeZoneServiceMock.Object,
                salesforceUserPublishServiceMock.Object);

            var salesforceFranchises = new[]
            {
                200
            };

            var token = "TOKEN";
            tokenServiceMock.Setup(mock => mock.RetrieveToken())
                .ReturnsAsync(token)
                .Verifiable();
            programDataServiceMock.Setup(mock => mock.RetrieveFranchises(ProgramDataSources.Salesforce, token, true))
                .ReturnsAsync(salesforceFranchises)
                .Verifiable();

            var salesforceUser = new AzureActiveDirectoryUser
            {
                Address = "11218 John Galt Blvd.",
                FranchiseNumber = "100"
            };

            // ACT
            await unitUnderTest.ProcessUser(salesforceUser);

            // ASSERT
            tokenServiceMock.Verify();
            programDataServiceMock.Verify();
            loggerMock.Verify(mock => mock.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(v => v.ToString().Contains("will be published to Salesforce.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Never);
            timeZoneServiceMock.Verify();
        }

        [TestMethod]
        public async Task ProcessMessage_ExceptionRetrievingSalesforceFranchiseData()
        {
            // ARRANGE
            var loggerMock = new Mock<ILogger<SalesforceMessageProcessor>>();
            var tokenServiceMock = new Mock<IHISCTokenService>();
            var programDataServiceMock = new Mock<IProgramDataService>();
            var timeZoneServiceMock = new Mock<ITimeZoneService>();
            var salesforceUserPublishServiceMock = new Mock<ISalesforceUserPublishService>();

            var unitUnderTest = new SalesforceMessageProcessor(loggerMock.Object,
                tokenServiceMock.Object,
                programDataServiceMock.Object,
                timeZoneServiceMock.Object,
                salesforceUserPublishServiceMock.Object);

            tokenServiceMock.Setup(mock => mock.RetrieveToken())
                .Throws<System.IO.IOException>()
                .Verifiable();

            var salesforceUser = new AzureActiveDirectoryUser
            {
                Address = "11218 John Galt Blvd.",
                FranchiseNumber = "100"
            };

            // ACT
            await unitUnderTest.ProcessUser(salesforceUser);

            // ASSERT
            tokenServiceMock.Verify();
            programDataServiceMock.Verify();
            loggerMock.Verify(mock => mock.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(v => v.ToString().Contains("will be published to Salesforce.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Never);
            timeZoneServiceMock.Verify();
        }

        [TestMethod]
        public async Task ProcessMessage_ExceptionRetrievingClearCareFranchiseData()
        {
            // ARRANGE
            var loggerMock = new Mock<ILogger<SalesforceMessageProcessor>>();
            var tokenServiceMock = new Mock<IHISCTokenService>();
            var programDataServiceMock = new Mock<IProgramDataService>();
            var timeZoneServiceMock = new Mock<ITimeZoneService>();
            var salesforceUserPublishServiceMock = new Mock<ISalesforceUserPublishService>();

            var unitUnderTest = new SalesforceMessageProcessor(loggerMock.Object,
                tokenServiceMock.Object,
                programDataServiceMock.Object,
                timeZoneServiceMock.Object,
                salesforceUserPublishServiceMock.Object);

            var salesforceFranchises = new[]
            {
                100
            };

            var token = "TOKEN";
            var sequence = new MockSequence();
            tokenServiceMock.InSequence(sequence).Setup(mock => mock.RetrieveToken()).ReturnsAsync(() => token).Verifiable();
            tokenServiceMock.InSequence(sequence).Setup(mock => mock.RetrieveToken()).Throws<System.IO.IOException>().Verifiable();

            programDataServiceMock.Setup(mock => mock.RetrieveFranchises(ProgramDataSources.Salesforce, token, true))
                .ReturnsAsync(salesforceFranchises)
                .Verifiable();

            var salesforceUser = new AzureActiveDirectoryUser
            {
                Address = "11218 John Galt Blvd.",
                FranchiseNumber = "100"
            };

            // ACT
            await unitUnderTest.ProcessUser(salesforceUser);

            // ASSERT
            tokenServiceMock.Verify();
            programDataServiceMock.Verify();
            loggerMock.Verify(mock => mock.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(v => v.ToString().Contains("will be published to Salesforce.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Never);
            timeZoneServiceMock.Verify();
        }
    }
}