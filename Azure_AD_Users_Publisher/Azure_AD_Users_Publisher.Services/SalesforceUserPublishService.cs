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
    public class SalesforceUserService : ISalesforceUserService
    {
        private readonly CacheControlHeaderValue _noCacheControlHeaderValue = new CacheControlHeaderValue {NoCache = true};

        private readonly ILogger<SalesforceUserService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISalesforceTokenService _tokenService;
        private readonly string _publishUrl;
        private readonly string _queryUrl;
        private readonly string _salesforceFranchisesUrl;
        
        private int _errorCount;
        private int _deactivationCount;
        private int _publishCount;

        public int PublishCount => _publishCount;
        public int DeactivationCount => _deactivationCount;
        public int ErrorCount => _errorCount;

        public SalesforceUserService(ILogger<SalesforceUserService> logger, 
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration, 
            ISalesforceTokenService tokenService)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _publishUrl = $"{configuration["SalesforceBaseUrl"]}{configuration["SalesforcePublishUrl"]}";
            _queryUrl = $"{configuration["SalesforceBaseUrl"]}{configuration["SalesforceQueryUrl"]}";
            _salesforceFranchisesUrl = $"{configuration["SalesforceBaseUrl"]}{configuration["SalesforceFranchisesUrl"]}";
            _tokenService = tokenService;
        }

        private async Task<HttpClient> GetHttpClient()
        {
            var client = _httpClientFactory.CreateClient("SalesforceHttpClient");
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
                throw new UnexpectedStatusCodeException(responseMessage);
            }

            Interlocked.Increment(ref _deactivationCount);
            _logger.LogDebug($"{correlationId}, Successfully Deactivated Salesforce User ExternalId: {externalId}");
        }

        public async Task<SalesforceQueryResponse> RetrieveAllUsers()
        {
            var client = await GetHttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, _queryUrl);
            requestMessage.Headers.CacheControl = _noCacheControlHeaderValue;
            
            var responseMessage = await client.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var message = $"Non Success Status Code when Retrieving All Salesforce Users, Response Content: {responseContent}";
                _logger.LogError(message);
                throw new UnexpectedStatusCodeException(responseMessage);
            }

            var json = await responseMessage.Content.ReadAsStringAsync();
            _logger.LogDebug($"Retrieved All Franchise Salesforce Users: {json}");
            var response = System.Text.Json.JsonSerializer.Deserialize<SalesforceQueryResponse>(json);
            return response;
        }
        
        public async Task<SalesforceQueryResponse> RetrieveAllFranchises()
        {
            // todo: is the response okay for this?
            var client = await GetHttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, _salesforceFranchisesUrl);
            requestMessage.Headers.CacheControl = _noCacheControlHeaderValue;
            
            var responseMessage = await client.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var message = $"Non Success Status Code when Retrieving All Salesforce Franchises, Response Content: {responseContent}";
                _logger.LogError(message);
                throw new UnexpectedStatusCodeException(responseMessage);
            }

            var json = await responseMessage.Content.ReadAsStringAsync();
            _logger.LogDebug($"Retrieved All Franchise Salesforce Franchises: {json}");
            var response = System.Text.Json.JsonSerializer.Deserialize<SalesforceQueryResponse>(json);
            return response;
        }
    }
}