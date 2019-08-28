using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AD_Users_Extract.Services.Exceptions;
using AD_Users_Extract.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AD_Users_Extract.Services
{
    public class GraphApiService : IGraphApiService
    {
        private const string _graphUrl = "https://graph.microsoft.com";
        private const string _bearerHeaderSchemeName = "Bearer";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _azureActiveDirectoryInstanceFormat;

        public GraphApiService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _azureActiveDirectoryInstanceFormat = config["AzureActiveDirectoryInstanceFormat"];
        }

        // todo: this method is not testable due to usage of requiring a valid AuthenticationContext, refactor later.
        public async Task<string> AcquireToken(string clientId, string appKey, string tenant)
        {
            var authority = string.Format(CultureInfo.InvariantCulture, _azureActiveDirectoryInstanceFormat, tenant);
            var authContext = new AuthenticationContext(authority);

            // Acquiring the token by using microsoft graph api resource
            var result = await authContext.AcquireTokenAsync(_graphUrl, new ClientCredential(clientId, appKey));     
            var token = result.AccessToken;
            return token ?? throw new ArgumentNullException(nameof(token));
        }

        private async Task<HttpResponseMessage> SendAsync(string url, string token)
        {
            var client = _httpClientFactory.CreateClient("GraphApiHttpClient");

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(_bearerHeaderSchemeName, token);
            var responseMessage = await client.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new UnexpectedStatusCodeException(responseMessage);
            }

            if (responseMessage.Content == null)
            {
                throw new UnexpectedDataException(nameof(responseMessage.Content));
            }

            return responseMessage;
        }

        public async Task<string> RetrieveData(string url, string token)
        {
            var responseMessage = await SendAsync(url, token);
            return await responseMessage.Content.ReadAsStringAsync();
        }
    }
}