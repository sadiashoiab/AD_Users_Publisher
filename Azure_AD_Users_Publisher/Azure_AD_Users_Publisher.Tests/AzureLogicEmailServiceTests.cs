using System.Net.Http;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services;
using Azure_AD_Users_Shared.Stubs;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Azure_AD_Users_Publisher.Tests
{
    [TestClass]
    public class AzureLogicEmailServiceTests
    {
        private static TestContext _context;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _context = context;
            _context.Properties["configuration"] = new ConfigurationBuilder()
                .AddJsonFile("azure-logic-email-service-test-settings.json")
                .Build();
        }

        [TestMethod]
        public async Task SendAlert_Success()
        {
            // ARRANGE
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var configuration = (IConfiguration) _context.Properties["configuration"];
            var unitUnderTest = new AzureLogicEmailService(configuration, httpClientFactoryMock.Object);

            var clientHandlerStub = new DelegatingHandlerStub();
            var client = new HttpClient(clientHandlerStub);

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>()))
                .Returns(client).Verifiable();

            // ACT
            await unitUnderTest.SendAlert("this is a sample alert from a hardcoded run");
            client.Dispose();

            // ASSERT
            httpClientFactoryMock.Verify();
        }
    }
}