using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services;
using Azure_AD_Users_Publisher.Services.Models;
using Azure_AD_Users_Shared.Models;
using LazyCache;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Azure_AD_Users_Publisher.Tests
{
    [TestClass]
    public class SalesforceMessageProcessorTests
    {
        private static TestContext _context;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _context = context;
            _context.Properties["appCache"] = new CachingService();
            //var appCache = (IAppCache) _context.Properties["appCache"];
        }

        [TestMethod]
        public async Task ProcessMessage_SuccessSyncUserWithNonClearCareOperatingSystem()
        {
            // ARRANGE
            var appCache = (IAppCache) _context.Properties["appCache"];
            var loggerMock = new Mock<ILogger<SalesforceMessageProcessor>>();
            var tokenServiceMock = new Mock<IHISCTokenService>();
            var programDataServiceMock = new Mock<IProgramDataService>();
            var timeZoneServiceMock = new Mock<ITimeZoneService>();
            var salesforceUserPublishServiceMock = new Mock<ISalesforceUserService>();

            var unitUnderTest = new SalesforceMessageProcessor(appCache,
                loggerMock.Object,
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
            //loggerMock.Verify(mock => mock.Log(
            //        LogLevel.Information,
            //        It.IsAny<EventId>(),
            //        It.Is<FormattedLogValues>(v => v.ToString().Contains("will be Published")),
            //        It.IsAny<Exception>(),
            //        It.IsAny<Func<object, Exception, string>>()),
            //    Times.Once);
            timeZoneServiceMock.Verify();
            salesforceUserPublishServiceMock.Verify(mock => mock.Publish(It.IsAny<SalesforceUser>()));
        }

        [TestMethod]
        public async Task ProcessMessage_SuccessSyncUserWithClearCareOperatingSystem()
        {
            // ARRANGE
            var appCache = (IAppCache) _context.Properties["appCache"];
            var loggerMock = new Mock<ILogger<SalesforceMessageProcessor>>();
            var tokenServiceMock = new Mock<IHISCTokenService>();
            var programDataServiceMock = new Mock<IProgramDataService>();
            var timeZoneServiceMock = new Mock<ITimeZoneService>();
            var salesforceUserPublishServiceMock = new Mock<ISalesforceUserService>();

            var unitUnderTest = new SalesforceMessageProcessor(appCache,
                loggerMock.Object,
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
            //loggerMock.Verify(mock => mock.Log(
            //        LogLevel.Information,
            //        It.IsAny<EventId>(),
            //        It.Is<FormattedLogValues>(v => v.ToString().Contains("will be Published")),
            //        It.IsAny<Exception>(),
            //        It.IsAny<Func<object, Exception, string>>()),
            //    Times.Once);
            timeZoneServiceMock.Verify();
            salesforceUserPublishServiceMock.Verify(mock => mock.Publish(It.IsAny<SalesforceUser>()));
        }

        [TestMethod]
        public async Task ProcessMessage_SuccessAndDoNotSyncUser()
        {
            // ARRANGE
            var appCache = (IAppCache) _context.Properties["appCache"];
            var loggerMock = new Mock<ILogger<SalesforceMessageProcessor>>();
            var tokenServiceMock = new Mock<IHISCTokenService>();
            var programDataServiceMock = new Mock<IProgramDataService>();
            var timeZoneServiceMock = new Mock<ITimeZoneService>();
            var salesforceUserPublishServiceMock = new Mock<ISalesforceUserService>();

            var unitUnderTest = new SalesforceMessageProcessor(appCache,
                loggerMock.Object,
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
            //loggerMock.Verify(mock => mock.Log(
            //        LogLevel.Information,
            //        It.IsAny<EventId>(),
            //        It.Is<FormattedLogValues>(v => v.ToString().Contains("will be published to Salesforce.")),
            //        It.IsAny<Exception>(),
            //        It.IsAny<Func<object, Exception, string>>()),
            //    Times.Never);
            timeZoneServiceMock.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(System.IO.IOException))]
        public async Task ProcessMessage_ExceptionRetrievingSalesforceFranchiseData()
        {
            // ARRANGE
            var appCache = (IAppCache) _context.Properties["appCache"];
            var loggerMock = new Mock<ILogger<SalesforceMessageProcessor>>();
            var tokenServiceMock = new Mock<IHISCTokenService>();
            var programDataServiceMock = new Mock<IProgramDataService>();
            var timeZoneServiceMock = new Mock<ITimeZoneService>();
            var salesforceUserPublishServiceMock = new Mock<ISalesforceUserService>();

            var unitUnderTest = new SalesforceMessageProcessor(appCache,
                loggerMock.Object,
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
        }

        [TestMethod]
        [ExpectedException(typeof(System.IO.IOException))]
        public async Task ProcessMessage_ExceptionRetrievingClearCareFranchiseData()
        {
            // ARRANGE
            var appCache = (IAppCache) _context.Properties["appCache"];
            var loggerMock = new Mock<ILogger<SalesforceMessageProcessor>>();
            var tokenServiceMock = new Mock<IHISCTokenService>();
            var programDataServiceMock = new Mock<IProgramDataService>();
            var timeZoneServiceMock = new Mock<ITimeZoneService>();
            var salesforceUserPublishServiceMock = new Mock<ISalesforceUserService>();

            var unitUnderTest = new SalesforceMessageProcessor(appCache,
                loggerMock.Object,
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
        }
    }
}