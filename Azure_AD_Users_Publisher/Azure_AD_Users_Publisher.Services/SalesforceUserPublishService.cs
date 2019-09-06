using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;
using Azure_AD_Users_Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Azure_AD_Users_Publisher.Services
{
    public class SalesforceUserPublishService : ISalesforceUserPublishService
    {
        private readonly CacheControlHeaderValue _noCacheControlHeaderValue = new CacheControlHeaderValue {NoCache = true};

        private readonly ILogger<SalesforceUserPublishService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _publishUrl;
        private readonly ISalesforceTokenService _tokenService;

        public SalesforceUserPublishService(ILogger<SalesforceUserPublishService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration, ISalesforceTokenService tokenService)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _publishUrl = configuration["SalesforcePublishUrl"];
            _tokenService = tokenService;
        }

        private async Task<HttpClient> GetHttpClient()
        {
            var client = _httpClientFactory.CreateClient("SalesforcePublishHttpClient");
            var bearerToken = await _tokenService.RetrieveToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            return client;
        }

        public async Task Publish(SalesforceUser user)
        {
            var client = await GetHttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, _publishUrl);
            requestMessage.Headers.CacheControl = _noCacheControlHeaderValue;

            var json = System.Text.Json.JsonSerializer.Serialize(user);
            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestMessage.Content = content;
            
            var correlationId = Guid.NewGuid();
            _logger.LogDebug($"{correlationId}, Publishing User to Salesforce: {json}");

            var responseMessage = await client.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                try
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    _logger.LogError($"{correlationId}, Non Success Status Code when Publishing User to Salesforce Response Content: {responseContent}, for User: {json}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{correlationId}, Exception when Publishing User: {json} to Salesforce. StackTrace: {ex.StackTrace}");
                }
            } 
            else
            {
                _logger.LogDebug($"{correlationId}, Successfully Published to Salesforce User: {json}");
            }
        }

        public async Task DeactivateUser(AzureActiveDirectoryUser user)
        {
            var client = await GetHttpClient();

            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"{_publishUrl}{user.ExternalId}");
            requestMessage.Headers.CacheControl = _noCacheControlHeaderValue;

            var responseMessage = await client.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(user);
                _logger.LogError($"Unexpected Status Code: {responseMessage.StatusCode} returned when Deactivating User to Salesforce. User ID: {user.ExternalId}, User: {json}");

                try
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    _logger.LogError($"Deactivating User to Salesforce Response Content: {responseContent}, for User: {json}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception when Deactivating User to Salesforce. StackTrace: {ex.StackTrace}");
                }
            }
        }
    }
}