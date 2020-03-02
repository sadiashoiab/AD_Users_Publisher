using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure_AD_Users_Extract.Services;
using Azure_AD_Users_Shared.Exceptions;
using Azure_AD_Users_Shared.Stubs;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Moq;

namespace Azure_AD_Users_Extract.Tests
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
            var httpClientFactoryMock = new Mock<System.Net.Http.IHttpClientFactory>();
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
            client.Dispose();

            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(_goodResponseString, result);
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedStatusCodeException))]
        public async Task RetrieveDataWithUnsuccessfulResponse()
        {
            // ARRANGE
            var configurationMock = new Mock<IConfiguration>();
            var httpClientFactoryMock = new Mock<System.Net.Http.IHttpClientFactory>();
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
            var httpClientFactoryMock = new Mock<System.Net.Http.IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub();
            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new GraphApiService(configurationMock.Object, httpClientFactoryMock.Object);

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>())).Returns(client);

            // ACT
            var _ = await unitUnderTest.RetrieveData("https://www.google.com", "Does not matter for this test as we are using a DelegatingHandler");
        }

        //[TestMethod]
        //public async Task Test()
        //{
        //    var clientId = "7495e03d-bfdd-4b49-86fd-93da77b69a03";
        //    var appKey = "uuo6FoRQF92X9T/PyBHym29jE0sB7TFYe+vpsMeZFnU="; // this is an expired secret, please use a valid secret to run this test.
        //    var authority = string.Format(CultureInfo.InvariantCulture, "https://login.microsoftonline.com/{0}", "849fc475-5645-4049-afc1-bcad4289b7ac");
        //    var authContext = new AuthenticationContext(authority);

        //    // Acquiring the token by using microsoft graph api resource
        //    var result = await authContext.AcquireTokenAsync("https://graph.microsoft.com", new ClientCredential(clientId, appKey));     
        //    var token = result.AccessToken;
        //    Assert.IsNotNull(token);
        //}
    }
}
