using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AD_Users_Extract.Services;
using AD_Users_Extract.Services.Exceptions;
using AD_Users_Extract.Tests.Stubs;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AD_Users_Extract.Tests
{
    [TestClass]
    public class GraphApiServiceTests
    {
        private readonly string _goodResponseString = "Good Response String";
        private readonly string _badResponseString = "Bad Response String";

        [TestMethod]
        public async Task RetrieveDataWithSuccessfulResponseAndData()
        {
            // ARRANGE
            var configurationMock = new Mock<IConfiguration>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(_goodResponseString, Encoding.UTF8)
                    };
                    return Task.FromResult(response);
                }
            );
            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new GraphApiService(configurationMock.Object, httpClientFactoryMock.Object);

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>())).Returns(client);

            // ACT
            var result = await unitUnderTest.RetrieveData("https://www.google.com", "Does not matter for this test as we are using a DelegatingHandler");

            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(_goodResponseString, result);
            client.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedStatusCodeException))]
        public async Task RetrieveDataWithUnsuccessfulResponse()
        {
            // ARRANGE
            var configurationMock = new Mock<IConfiguration>();
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
            var unitUnderTest = new GraphApiService(configurationMock.Object, httpClientFactoryMock.Object);

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>())).Returns(client);

            // ACT
            var _ = await unitUnderTest.RetrieveData("https://www.google.com", "Does not matter for this test as we are using a DelegatingHandler");
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedDataException))]
        public async Task RetrieveData_SuccessfulResponseAndUnexpectedDataResponse()
        {
            // ARRANGE
            var configurationMock = new Mock<IConfiguration>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub();
            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new GraphApiService(configurationMock.Object, httpClientFactoryMock.Object);

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>())).Returns(client);

            // ACT
            var _ = await unitUnderTest.RetrieveData("https://www.google.com", "Does not matter for this test as we are using a DelegatingHandler");
        }
    }
}
