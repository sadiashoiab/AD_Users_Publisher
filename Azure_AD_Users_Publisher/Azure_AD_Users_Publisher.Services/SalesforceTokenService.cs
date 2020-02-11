using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;
using Azure_AD_Users_Shared.Exceptions;
using Azure_AD_Users_Shared.Services;
using LazyCache;
using Microsoft.Extensions.Configuration;

namespace Azure_AD_Users_Publisher.Services
{
    public class SalesforceTokenService : ISalesforceTokenService
    {
        private const string _cacheKeyPrefix = "_SalesforceTokenService_";

        private readonly CacheControlHeaderValue _noCacheControlHeaderValue =
            new CacheControlHeaderValue {NoCache = true};

        private readonly IAppCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        private readonly string _tokenUrl;

        public SalesforceTokenService(IAppCache cache, IHttpClientFactory httpClientFactory, IAzureKeyVaultService azureKeyVaultService, IConfiguration configuration)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _azureKeyVaultService = azureKeyVaultService;
            _tokenUrl = configuration["SalesforceTokenUrl"];
        }

        private async Task<string> GetClientBody()
        {
            var clientIdTask = _azureKeyVaultService.GetSecret("SalesforceTokenClientId");
            var clientSecretTask = _azureKeyVaultService.GetSecret("SalesforceTokenClientSecret");
            var clientUsernameTask = _azureKeyVaultService.GetSecret("SalesforceTokenUsername");
            var clientPasswordTask = _azureKeyVaultService.GetSecret("SalesforceTokenPassword");
            await Task.WhenAll(clientIdTask, clientSecretTask, clientUsernameTask, clientPasswordTask);

            return $"grant_type=password&client_id={await clientIdTask}&client_secret={await clientSecretTask}&username={await clientUsernameTask}&password={await clientPasswordTask}";
        }

        public async Task<string> RetrieveToken()
        {
            var accessToken = await _cache.GetOrAddAsync($"{_cacheKeyPrefix}AccessToken", GetAccessToken, DateTimeOffset.Now.AddMinutes(20));
            return accessToken;
        }

        private async Task<string> GetAccessToken()
        {
            var client = _httpClientFactory.CreateClient("TokenApiHttpClient");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _tokenUrl);
            requestMessage.Headers.CacheControl = _noCacheControlHeaderValue;

            var contentBody = await _cache.GetOrAddAsync($"{_cacheKeyPrefix}ContentBody", GetClientBody);
            var content = new StringContent(contentBody);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            requestMessage.Content = content;

            var responseMessage = await client.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new UnexpectedStatusCodeException(responseMessage);
            }

            if (responseMessage.Content == null)
            {
                throw new UnexpectedDataException(nameof(responseMessage.Content));
            }

            var json = await responseMessage.Content.ReadAsStringAsync();
            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<SalesforceTokenResponse>(json);

            return tokenResponse.access_token;
        }
    }
}