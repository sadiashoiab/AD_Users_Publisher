﻿using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;
using Azure_AD_Users_Shared.Exceptions;
using Azure_AD_Users_Shared.Services;
using LazyCache;
using Microsoft.Extensions.Configuration;

namespace Azure_AD_Users_Publisher.Services
{
    public class HISCTokenService : IHISCTokenService
    {
        private const string _cacheKey = "_HISCToken";
        
        private readonly CacheControlHeaderValue _noCacheControlHeaderValue = new CacheControlHeaderValue {NoCache = true};
        private readonly IAppCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        private readonly string _resourceUrl;
        private readonly string _tokenUrl;

        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1,1);

        private string ContentBody { get; set; }

        public HISCTokenService(IAppCache cache, IHttpClientFactory httpClientFactory, IAzureKeyVaultService azureKeyVaultService, IConfiguration configuration)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _azureKeyVaultService = azureKeyVaultService;
            _resourceUrl = configuration["ProgramDataUrl"];
            _tokenUrl = configuration["HISCTokenUrl"];
        }

        private async Task<string> GetClientBody()
        {
            // note: check if it has been set, if so just give the value, otherwise...
            if (string.IsNullOrWhiteSpace(ContentBody))
            {
                // note: technically we could have many callers... use a lightweight semaphore to only allow one at a time for setting
                await _semaphoreSlim.WaitAsync();
                try
                {
                    // note: checking again, because caller other than myself could, in theory, have set it while we were waiting for semaphore...
                    //       and if so, no need to get it again here
                    if (string.IsNullOrWhiteSpace(ContentBody))
                    {
                        var clientIdTask = _azureKeyVaultService.GetSecret("BearerTokenClientId");
                        var clientSecretTask = _azureKeyVaultService.GetSecret("BearerTokenClientSecret");
                        await Task.WhenAll(clientIdTask, clientSecretTask);

                        ContentBody = $"client_id={await clientIdTask}&client_secret={await clientSecretTask}&grant_type=client_credentials&resource={_resourceUrl}";
                    }
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }

            return ContentBody;
        }

        private async Task<HISCTokenResponse> RequestBearerToken()
        {
            var client = _httpClientFactory.CreateClient("TokenApiHttpClient");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _tokenUrl);
            requestMessage.Headers.CacheControl = _noCacheControlHeaderValue;

            var contentBody = await GetClientBody();
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
            var bearerTokenResponse = System.Text.Json.JsonSerializer.Deserialize<HISCTokenResponse>(json);
            return bearerTokenResponse;
        }

        public async Task<string> RetrieveToken()
        {
            // note: using the default caching duration of 20 minutes
            var token = await _cache.GetOrAddAsync(_cacheKey, RequestBearerToken);
            return token.access_token;
        }
    }
}