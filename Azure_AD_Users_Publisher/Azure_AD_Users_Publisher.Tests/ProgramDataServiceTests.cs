using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services;
using Azure_AD_Users_Publisher.Services.Exceptions;
using Azure_AD_Users_Publisher.Tests.Stubs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Azure_AD_Users_Publisher.Tests
{
    [TestClass]
    public class ProgramDataServiceTests
    {
        private static TestContext _context;
        private readonly string _badResponseString = "Bad Response String";


        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _context = context;
            _context.Properties["configuration"] = new ConfigurationBuilder()
                .AddJsonFile("program-data-service-test-settings.json")
                .Build();
        }

        [TestMethod]
        public async Task RetrieveFranchises_Salesforce_Success()
        { 
            // ARRANGE
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var configuration = (IConfiguration) _context.Properties["configuration"];
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var jsonResponse = "[100,9999,997,713,101,169,3009,235]";

            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(jsonResponse, Encoding.UTF8)
                    };
                    return Task.FromResult(response);
                }
            );

            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new ProgramDataService(memoryCache, configuration, httpClientFactoryMock.Object);

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>()))
                .Returns(client).Verifiable();

            var bearerToken = "SAMPLETOKEN";

            // ACT
            var results = await unitUnderTest.RetrieveFranchises(ProgramDataSources.Salesforce, bearerToken);
            client.Dispose();

            // ASSERT
            httpClientFactoryMock.Verify();
            Assert.IsNotNull(results);
            Assert.AreEqual(8, results.Length);
            Assert.AreEqual(100, results.First());
            Assert.AreEqual(235, results.Last());
        }

        [TestMethod]
        public async Task RetrieveFranchises_ClearCare_Success()
        { 
            // ARRANGE
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var configuration = (IConfiguration) _context.Properties["configuration"];
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var jsonResponse = "[100,9999,997,713,101,169,3009]";

            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(jsonResponse, Encoding.UTF8)
                    };
                    return Task.FromResult(response);
                }
            );

            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new ProgramDataService(memoryCache, configuration, httpClientFactoryMock.Object);

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>()))
                .Returns(client).Verifiable();

            var bearerToken = "SAMPLETOKEN";

            // ACT
            var results = await unitUnderTest.RetrieveFranchises(ProgramDataSources.ClearCare, bearerToken);
            client.Dispose();

            // ASSERT
            httpClientFactoryMock.Verify();
            Assert.IsNotNull(results);
            Assert.AreEqual(7, results.Length);
            Assert.AreEqual(100, results.First());
            Assert.AreEqual(3009, results.Last());
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedStatusCodeException))]
        public async Task RetrieveFranchises_UnsuccessfulResponse()
        {
            // ARRANGE
            var memoryCacheMock = new Mock<IMemoryCache>();
            var configuration = (IConfiguration) _context.Properties["configuration"];
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();

            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(_badResponseString, Encoding.UTF8)
                };
                
                return Task.FromResult(response);
            });
            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new ProgramDataService(memoryCacheMock.Object, configuration, httpClientFactoryMock.Object);

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>()))
                .Returns(client).Verifiable();

            var bearerToken = "SAMPLETOKEN";

            // ACT
            var _ = await unitUnderTest.RetrieveFranchises(ProgramDataSources.ClearCare, bearerToken);
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedDataException))]
        public async Task RetrieveFranchises_SuccessfulResponseAndUnexpectedDataResponse()
        {
            // ARRANGE
            var memoryCacheMock = new Mock<IMemoryCache>();
            var configuration = (IConfiguration) _context.Properties["configuration"];
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub();
            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new ProgramDataService(memoryCacheMock.Object, configuration, httpClientFactoryMock.Object);

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>()))
                .Returns(client).Verifiable();

            var bearerToken = "SAMPLETOKEN";

            // ACT
            var _ = await unitUnderTest.RetrieveFranchises(ProgramDataSources.ClearCare, bearerToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task RetrieveFranchises_ArgumentOutOfRangeException()
        {
            // ARRANGE
            var memoryCacheMock = new Mock<IMemoryCache>();
            var configuration = (IConfiguration) _context.Properties["configuration"];
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var unitUnderTest = new ProgramDataService(memoryCacheMock.Object, configuration, httpClientFactoryMock.Object);
            var bearerToken = "SAMPLETOKEN";

            // ACT
            var _ = await unitUnderTest.RetrieveFranchises(ProgramDataSources.Invalid, bearerToken);
        }

        [TestMethod]
        public void EnumExtension_GetDescription()
        {
            // ARRANGE
            var noDescriptionEnumValue = ProgramDataSources.Invalid;

            // ACT
            var description = noDescriptionEnumValue.GetDescription();

            // ASSERT
            Assert.IsTrue(string.IsNullOrWhiteSpace(description));
        }
    }
}
