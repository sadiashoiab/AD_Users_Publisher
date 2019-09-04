﻿using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services;
using Azure_AD_Users_Shared.Models;
using Microsoft.Azure.ServiceBus;
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
            programDataServiceMock.Setup(mock => mock.RetrieveFranchises(ProgramDataSources.Salesforce, token))
                .ReturnsAsync(salesforceFranchises)
                .Verifiable();
            programDataServiceMock.Setup(mock => mock.RetrieveFranchises(ProgramDataSources.ClearCare, token))
                .ReturnsAsync(clearCareFranchises)
                .Verifiable();

            var timeZone = "Americas/NewYork";
            timeZoneServiceMock.Setup(mock => mock.RetrieveTimeZone(It.IsAny<AzureActiveDirectoryUser>()))
                .ReturnsAsync(timeZone)
                .Verifiable();

            var subscriptionClientMock = new Mock<ISubscriptionClient>();
            var salesforceUser = new AzureActiveDirectoryUser
            {
                Address = "11218 John Galt Blvd.",
                FranchiseNumber = "100"
            };
            var salesforceUserJson = System.Text.Json.JsonSerializer.Serialize(salesforceUser);
            var message = new Message(Encoding.UTF8.GetBytes(salesforceUserJson));
            var cancellationToken = new CancellationToken();

            subscriptionClientMock.Setup(mock => mock.CompleteAsync(null))
                .Returns(Task.CompletedTask).Verifiable();

            // ACT
            await unitUnderTest.ProcessMessage(subscriptionClientMock.Object, message, cancellationToken);

            // ASSERT
            tokenServiceMock.Verify();
            programDataServiceMock.Verify();
            loggerMock.Verify(mock => mock.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(v => v.ToString().Contains("will be Published.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
            timeZoneServiceMock.Verify();
            // todo: add verify back once allowed to call and invoke has been re-enabled
            //salesforceUserPublishServiceMock.Verify(mock => mock.Publish(It.IsAny<AzureActiveDirectoryUser>()));
            subscriptionClientMock.Verify();
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
            programDataServiceMock.Setup(mock => mock.RetrieveFranchises(ProgramDataSources.Salesforce, token))
                .ReturnsAsync(salesforceFranchises)
                .Verifiable();
            programDataServiceMock.Setup(mock => mock.RetrieveFranchises(ProgramDataSources.ClearCare, token))
                .ReturnsAsync(clearCareFranchises)
                .Verifiable();

            var timeZone = "Americas/NewYork";
            timeZoneServiceMock.Setup(mock => mock.RetrieveTimeZone(It.IsAny<AzureActiveDirectoryUser>()))
                .ReturnsAsync(timeZone)
                .Verifiable();

            var subscriptionClientMock = new Mock<ISubscriptionClient>();
            var salesforceUser = new AzureActiveDirectoryUser
            {
                Address = "11218 John Galt Blvd.",
                FranchiseNumber = "100"
            };
            var salesforceUserJson = System.Text.Json.JsonSerializer.Serialize(salesforceUser);
            var message = new Message(Encoding.UTF8.GetBytes(salesforceUserJson));
            var cancellationToken = new CancellationToken();

            subscriptionClientMock.Setup(mock => mock.CompleteAsync(null))
                .Returns(Task.CompletedTask).Verifiable();

            // ACT
            await unitUnderTest.ProcessMessage(subscriptionClientMock.Object, message, cancellationToken);

            // ASSERT
            tokenServiceMock.Verify();
            programDataServiceMock.Verify();
            loggerMock.Verify(mock => mock.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(v => v.ToString().Contains("will be Published.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
            timeZoneServiceMock.Verify();
            // todo: add verify back once allowed to call and invoke has been re-enabled
            //salesforceUserPublishServiceMock.Verify(mock => mock.Publish(It.IsAny<AzureActiveDirectoryUser>()));
            subscriptionClientMock.Verify();
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
            programDataServiceMock.Setup(mock => mock.RetrieveFranchises(ProgramDataSources.Salesforce, token))
                .ReturnsAsync(salesforceFranchises)
                .Verifiable();

            var subscriptionClientMock = new Mock<ISubscriptionClient>();
            var salesforceUser = new AzureActiveDirectoryUser
            {
                Address = "11218 John Galt Blvd.",
                FranchiseNumber = "100"
            };
            var salesforceUserJson = System.Text.Json.JsonSerializer.Serialize(salesforceUser);
            var message = new Message(Encoding.UTF8.GetBytes(salesforceUserJson));
            var cancellationToken = new CancellationToken();

            subscriptionClientMock.Setup(mock => mock.CompleteAsync(null))
                .Returns(Task.CompletedTask).Verifiable();

            // ACT
            await unitUnderTest.ProcessMessage(subscriptionClientMock.Object, message, cancellationToken);

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
            subscriptionClientMock.Verify();
        }
    }
}