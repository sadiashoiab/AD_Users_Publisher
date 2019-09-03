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

        public async Task Publish(SalesforceUser user)
        {
            var client = _httpClientFactory.CreateClient("SalesforcePublishHttpClient");
            var bearerToken = await _tokenService.RetrieveToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var requestMessage = new HttpRequestMessage(HttpMethod.Put, _publishUrl);
            requestMessage.Headers.CacheControl = _noCacheControlHeaderValue;

            var json = System.Text.Json.JsonSerializer.Serialize(user);
            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestMessage.Content = content;

            var responseMessage = await client.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                _logger.LogError($"Unexpected Status Code returned when Publishing User to Salesforce. User ID: {user.ExternalId}, Data: {json}");

                var responseContentJson = await responseMessage.Content.ReadAsStringAsync();
                var salesforcePublishResponse = System.Text.Json.JsonSerializer.Deserialize<SalesforcePublishResponse>(responseContentJson);
                _logger.LogError($"Salesforce Publish Response Error Message: {salesforcePublishResponse.error.errorMessage}, when publishing User: {json}");

                // todo: Sadia, what do you want to happen if the publish to Salesforce errors besides log the error?
                //       do you want to move the message that is on the service bus to deadletter, or do you want to keep it on the bus for later processing?
                //throw new UnexpectedStatusCodeException(responseMessage);
            }
        }
    }
}