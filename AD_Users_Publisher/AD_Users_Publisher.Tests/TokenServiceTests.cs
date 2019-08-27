using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AD_Users_Publisher.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AD_Users_Publisher.Tests
{
    [TestClass]
    public class TokenServiceTests
    {
        [TestMethod]
        public async Task RetrieveToken_Success()
        {
            // ARRANGE
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var keyVaultServiceMock = new Mock<IAzureKeyVaultService>();

            // todo: configure client to return what we expect so that we are not "really" going across the network
            //var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
            //    {
            //        var response = new HttpResponseMessage(HttpStatusCode.OK)
            //        {
            //            Content = new StringContent(geoCodeResult, Encoding.UTF8)
            //        };
            //        return Task.FromResult(response);
            //    }
            //);

            var client = new HttpClient();
            var unitUnderTest = new TokenService(httpClientFactoryMock.Object, keyVaultServiceMock.Object);

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>()))
                .Returns(client).Verifiable();

            var clientId = "ClientId";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("BarerTokenClientId"))
                .ReturnsAsync(() => clientId).Verifiable();
            
            var appKey = "ClientSecret";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("BarerTokenClientSecret"))
                .ReturnsAsync(() => appKey).Verifiable();

            // ACT
            var token = await unitUnderTest.RetrieveToken();

            // ASSERT
            httpClientFactoryMock.Verify();
            Assert.IsNotNull(token);
        }
    }
}
