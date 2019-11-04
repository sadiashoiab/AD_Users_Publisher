using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;
using Azure_AD_Users_Shared.Exceptions;
using LazyCache;
using Microsoft.Extensions.Configuration;

namespace Azure_AD_Users_Publisher.Services
{
    public class SalesforceTimeZoneManagerService : ISalesforceTimeZoneManagerService
    {
        private const string _cacheKeyPrefix = "_SalesforceTimeZoneManagerService_";

        private readonly IAppCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _timeZoneUrl;

        public SalesforceTimeZoneManagerService(IAppCache cache, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _timeZoneUrl = $"{configuration["SalesforceBaseUrl"]}{configuration["SalesforceTimeZoneUrl"]}";
        }

        public async Task<IList<SalesforceSupportedTimeZone>> GetSupportedTimeZones()
        {
            var salesforceTimeZones = await _cache.GetOrAddAsync(_cacheKeyPrefix, RetrieveSalesforceTimeZones);
            return salesforceTimeZones;
        }


        private async Task<IList<SalesforceSupportedTimeZone>> RetrieveSalesforceTimeZones()
        {
            var responseMessage = await SendAsync(_timeZoneUrl);
            var json = await responseMessage.Content.ReadAsStringAsync();
            // todo: use correct type
            var results = System.Text.Json.JsonSerializer.Deserialize<IList<SalesforceSupportedTimeZone>>(json);
            return results;
        }

        private async Task<HttpResponseMessage> SendAsync(string url)
        {
            var client = _httpClientFactory.CreateClient("SalesforceHttpClient");

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            var responseMessage = await client.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new UnexpectedStatusCodeException(responseMessage);
            }

            if (responseMessage.Content == null)
            {
                throw new UnexpectedDataException("No results from Salesforce TimeZone Manager");
            }

            return responseMessage;
        }
    }
}