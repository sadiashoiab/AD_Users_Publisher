using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;
using Azure_AD_Users_Shared.Exceptions;
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
        private readonly IAzureLogicEmailService _azureLogicEmailService;

        private int _errorCount;
        private int _deactivationCount;
        private int _publishCount;

        public int PublishCount => _publishCount;
        public int DeactivationCount => _deactivationCount;
        public int ErrorCount => _errorCount;

        public SalesforceUserPublishService(
            ILogger<SalesforceUserPublishService> logger, 
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration, 
            ISalesforceTokenService tokenService,
            IAzureLogicEmailService azureLogicEmailService)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _publishUrl = configuration["SalesforcePublishUrl"];
            _tokenService = tokenService;
            _azureLogicEmailService = azureLogicEmailService;
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
                Interlocked.Increment(ref _errorCount);

                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var message = $"{correlationId}, Non Success Status Code when Publishing User to Salesforce Response Content: {responseContent}, for User: {json}";
                _logger.LogError(message);
                await _azureLogicEmailService.SendAlert(message);
                throw new UnexpectedStatusCodeException(responseMessage);
            }

            Interlocked.Increment(ref _publishCount);
            _logger.LogDebug($"{correlationId}, Successfully Published to Salesforce User: {json}");
        }

        public async Task Deactivate(string externalId)
        {
            var client = await GetHttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"{_publishUrl}{externalId}");
            requestMessage.Headers.CacheControl = _noCacheControlHeaderValue;
            
            var correlationId = Guid.NewGuid();
            _logger.LogDebug($"{correlationId}, Deactivating Salesforce User ExternalId: {externalId}");

            var responseMessage = await client.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                Interlocked.Increment(ref _errorCount);

                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var message = $"{correlationId}, Non Success Status Code when Deactivating Salesforce User, Response Content: {responseContent}, for User ExternalId: {externalId}";
                _logger.LogError(message);
                await _azureLogicEmailService.SendAlert(message);
                throw new UnexpectedStatusCodeException(responseMessage);
            }

            Interlocked.Increment(ref _deactivationCount);
            _logger.LogDebug($"{correlationId}, Successfully Deactivated Salesforce User ExernalId: {externalId}");
        }
    }
}