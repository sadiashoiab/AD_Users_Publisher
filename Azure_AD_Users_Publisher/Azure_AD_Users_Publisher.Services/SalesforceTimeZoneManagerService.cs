using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private readonly ISalesforceTokenService _tokenService;
        private readonly string _timeZoneUrl;

        public SalesforceTimeZoneManagerService(IAppCache cache, IConfiguration configuration, IHttpClientFactory httpClientFactory, ISalesforceTokenService tokenService)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
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
            var salesforceTimeZones = System.Text.Json.JsonSerializer.Deserialize<IList<SalesforceTimeZone>>(json);
            var results = MapSalesforceTimeZones(salesforceTimeZones);
            return results;
        }

        private IList<SalesforceSupportedTimeZone> MapSalesforceTimeZones(IList<SalesforceTimeZone> salesforceTimeZones)
        {
            var salesforceSupportedTimeZones = new List<SalesforceSupportedTimeZone>();
            salesforceSupportedTimeZones.AddRange(salesforceTimeZones.Select(timezone => new SalesforceSupportedTimeZone(timezone.offsetSeconds, timezone.name, timezone.key)));
            return salesforceSupportedTimeZones;
        }

        private async Task<HttpClient> GetHttpClient()
        {
            var client = _httpClientFactory.CreateClient("SalesforceHttpClient");
            var bearerToken = await _tokenService.RetrieveToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            return client;
        }

        private async Task<HttpResponseMessage> SendAsync(string url)
        {
            var client = await GetHttpClient();
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