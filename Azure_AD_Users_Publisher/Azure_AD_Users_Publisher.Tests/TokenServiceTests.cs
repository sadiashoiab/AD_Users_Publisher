using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services;
using Azure_AD_Users_Publisher.Services.Exceptions;
using Azure_AD_Users_Publisher.Services.Interfaces;
using Azure_AD_Users_Publisher.Tests.Stubs;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Azure_AD_Users_Publisher.Tests
{
    [TestClass]
    public class TokenServiceTests
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
        public async Task RetrieveToken_Success()
        {
            // ARRANGE
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var keyVaultServiceMock = new Mock<IAzureKeyVaultService>();
            var configuration = (IConfiguration) _context.Properties["configuration"];
            var expectedAccessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImllX3FXQ1hoWHh0MXpJRXN1NGM3YWNRVkduNCIsImtpZCI6ImllX3FXQ1hoWHh0MXpJRXN1NGM3YWNRVkduNCJ9.eyJhdWQiOiJodHRwczovL2hpc2Nwcm9ncmFtZGF0YWR2LmF6dXJld2Vic2l0ZXMubmV0IiwiaXNzIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvNDNlNWRlYmEtMmM1NC00M2E0LTlhMTAtYzlmMTBiMWM2NmE1LyIsImlhdCI6MTU2Njk5NzUyNywibmJmIjoxNTY2OTk3NTI3LCJleHAiOjE1NjcwMDE0MjcsImFpbyI6IjQyRmdZR2grTGVPNmI2N09qMGx4VzdhL0NIa1NEUUE9IiwiYXBwaWQiOiJlNTBmMTE0Ny1lZGQxLTQ4NDItYmQzMS0wZjlmYTM1MjNhNjYiLCJhcHBpZGFjciI6IjEiLCJpZHAiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC80M2U1ZGViYS0yYzU0LTQzYTQtOWExMC1jOWYxMGIxYzY2YTUvIiwib2lkIjoiNzQxZWQ5OTUtM2UyOC00NWM3LWIyYzAtYzJiZTU5NGQwYjY0Iiwic3ViIjoiNzQxZWQ5OTUtM2UyOC00NWM3LWIyYzAtYzJiZTU5NGQwYjY0IiwidGlkIjoiNDNlNWRlYmEtMmM1NC00M2E0LTlhMTAtYzlmMTBiMWM2NmE1IiwidXRpIjoiZDF2dUNoWmp4MFNUalNZcjMyc0tBQSIsInZlciI6IjEuMCJ9.iERX4wBdjEouoNnCQFL4BrxCtRPID6P1WxQMGjxyRyYgF-ZX7vloB8Uw9j5bcLLO3u80C6W84vYct7IkHBrznzV2iRMe84yNTVbx9-87VvnJnilWiF0nHHWrBcnRqFJtckZM2qcUYbUMfDO_83fdoPaZAOUnc-h7qsu3MYXYxkBiNWpSQQbAgzIG4UM9eIYFYEnkBToGrOQz6NtB1JO00czfROYxFe2_dAGO8eQ5abRbHSwRLSxyzHQHXBfCKfw7S4CA6d0o-5Izo3HKxoYfxRUnBvDLdeIxs8LQDi5EFOv4rA0ioQOWskBNc6KiB5vyJjo0ZsBDjtc9Df_nEScbCw";
            var jsonTokenResponse = "{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"ext_expires_in\":\"3599\",\"expires_on\":\"1567001427\",\"not_before\":\"1566997527\",\"resource\":\"https://hiscprogramdatadv.azurewebsites.net\",\"access_token\":\"eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImllX3FXQ1hoWHh0MXpJRXN1NGM3YWNRVkduNCIsImtpZCI6ImllX3FXQ1hoWHh0MXpJRXN1NGM3YWNRVkduNCJ9.eyJhdWQiOiJodHRwczovL2hpc2Nwcm9ncmFtZGF0YWR2LmF6dXJld2Vic2l0ZXMubmV0IiwiaXNzIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvNDNlNWRlYmEtMmM1NC00M2E0LTlhMTAtYzlmMTBiMWM2NmE1LyIsImlhdCI6MTU2Njk5NzUyNywibmJmIjoxNTY2OTk3NTI3LCJleHAiOjE1NjcwMDE0MjcsImFpbyI6IjQyRmdZR2grTGVPNmI2N09qMGx4VzdhL0NIa1NEUUE9IiwiYXBwaWQiOiJlNTBmMTE0Ny1lZGQxLTQ4NDItYmQzMS0wZjlmYTM1MjNhNjYiLCJhcHBpZGFjciI6IjEiLCJpZHAiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC80M2U1ZGViYS0yYzU0LTQzYTQtOWExMC1jOWYxMGIxYzY2YTUvIiwib2lkIjoiNzQxZWQ5OTUtM2UyOC00NWM3LWIyYzAtYzJiZTU5NGQwYjY0Iiwic3ViIjoiNzQxZWQ5OTUtM2UyOC00NWM3LWIyYzAtYzJiZTU5NGQwYjY0IiwidGlkIjoiNDNlNWRlYmEtMmM1NC00M2E0LTlhMTAtYzlmMTBiMWM2NmE1IiwidXRpIjoiZDF2dUNoWmp4MFNUalNZcjMyc0tBQSIsInZlciI6IjEuMCJ9.iERX4wBdjEouoNnCQFL4BrxCtRPID6P1WxQMGjxyRyYgF-ZX7vloB8Uw9j5bcLLO3u80C6W84vYct7IkHBrznzV2iRMe84yNTVbx9-87VvnJnilWiF0nHHWrBcnRqFJtckZM2qcUYbUMfDO_83fdoPaZAOUnc-h7qsu3MYXYxkBiNWpSQQbAgzIG4UM9eIYFYEnkBToGrOQz6NtB1JO00czfROYxFe2_dAGO8eQ5abRbHSwRLSxyzHQHXBfCKfw7S4CA6d0o-5Izo3HKxoYfxRUnBvDLdeIxs8LQDi5EFOv4rA0ioQOWskBNc6KiB5vyJjo0ZsBDjtc9Df_nEScbCw\"}";

            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(jsonTokenResponse, Encoding.UTF8)
                    };
                    return Task.FromResult(response);
                }
            );

            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new TokenService(httpClientFactoryMock.Object, keyVaultServiceMock.Object, configuration);

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>()))
                .Returns(client).Verifiable();

            var clientId = "ClientId";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("BearerTokenClientId"))
                .ReturnsAsync(() => clientId).Verifiable();
            
            var appKey = "ClientSecret";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("BearerTokenClientSecret"))
                .ReturnsAsync(() => appKey).Verifiable();

            // ACT
            var token = await unitUnderTest.RetrieveToken();
            client.Dispose();

            // ASSERT
            httpClientFactoryMock.Verify();
            Assert.AreEqual(expectedAccessToken, token);
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedStatusCodeException))]
        public async Task RetrieveDataWithUnsuccessfulResponse()
        {
            // ARRANGE
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var keyVaultServiceMock = new Mock<IAzureKeyVaultService>();
            var configuration = (IConfiguration) _context.Properties["configuration"];
            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(_badResponseString, Encoding.UTF8)
                };
                
                return Task.FromResult(response);
            });

            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new TokenService(httpClientFactoryMock.Object, keyVaultServiceMock.Object, configuration);

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>()))
                .Returns(client).Verifiable();

            var clientId = "ClientId";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("BarerTokenClientId"))
                .ReturnsAsync(() => clientId).Verifiable();
            
            var appKey = "ClientSecret";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("BarerTokenClientSecret"))
                .ReturnsAsync(() => appKey).Verifiable();

            // ACT
            var _ = await unitUnderTest.RetrieveToken();
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedDataException))]
        public async Task RetrieveData_SuccessfulResponseAndUnexpectedDataResponse()
        {
            // ARRANGE
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var keyVaultServiceMock = new Mock<IAzureKeyVaultService>();
            var configuration = (IConfiguration) _context.Properties["configuration"];
            var clientHandlerStub = new DelegatingHandlerStub();
            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new TokenService(httpClientFactoryMock.Object, keyVaultServiceMock.Object, configuration);

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>()))
                .Returns(client).Verifiable();

            var clientId = "ClientId";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("BarerTokenClientId"))
                .ReturnsAsync(() => clientId).Verifiable();
            
            var appKey = "ClientSecret";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("BarerTokenClientSecret"))
                .ReturnsAsync(() => appKey).Verifiable();

            // ACT
            var _ = await unitUnderTest.RetrieveToken();
        }
    }
}
