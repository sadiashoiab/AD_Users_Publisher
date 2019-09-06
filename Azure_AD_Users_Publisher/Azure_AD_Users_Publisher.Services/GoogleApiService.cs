using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;
using Azure_AD_Users_Shared.Exceptions;
using Azure_AD_Users_Shared.Services;

namespace Azure_AD_Users_Publisher.Services
{
    public class GoogleApiService : IGoogleApiService
    {
        private const string _googleApiBaseUrl = "https://maps.googleapis.com/maps/api/";
        
        private readonly IAzureKeyVaultService _keyVaultService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UrlEncoder _urlEncoder = UrlEncoder.Default;

        private string GoogleApiKey { get; set; }

        public GoogleApiService(IAzureKeyVaultService keyVaultService, IHttpClientFactory httpClientFactory)
        {
            _keyVaultService = keyVaultService;
            _httpClientFactory = httpClientFactory;
        }

        private async Task<string> BuildApiUrl(GoogleApiEndpointEnum googleApiEndpointEnum, string query)
        {
            switch (googleApiEndpointEnum)
            {
                case GoogleApiEndpointEnum.GeoCode:
                case GoogleApiEndpointEnum.TimeZone:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(googleApiEndpointEnum));
            }

            if (string.IsNullOrWhiteSpace(GoogleApiKey))
            {
                GoogleApiKey = await _keyVaultService.GetSecret("GoogleApiKey");
            }

            return $"{_googleApiBaseUrl}{googleApiEndpointEnum.ToString().ToLower()}/json?{query}&key={GoogleApiKey}";
        }

        private async Task<HttpResponseMessage> SendAsync(string url)
        {
            var client = _httpClientFactory.CreateClient("GoogleApiHttpClient");

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
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

        public async Task<GoogleApiLocation> GeoCode(string streetAddress, string city, string state, string postalCode)
        {
            var address = $"{streetAddress}, {city}, {state}";
            var encodedAddress = _urlEncoder.Encode(address);
            var encodedPostalCode = _urlEncoder.Encode(postalCode);
            var query = $"{encodedAddress}&components=postal_code:{encodedPostalCode}";
            var url = await BuildApiUrl(GoogleApiEndpointEnum.GeoCode, query);
            var responseMessage = await SendAsync(url);
            var json = await responseMessage.Content.ReadAsStringAsync();
            var geoCodeResults = System.Text.Json.JsonSerializer.Deserialize<GoogleApiGeoCodeRootObject>(json);
            if (geoCodeResults.results == null || !geoCodeResults.results.Any())
            {
                var innerException = new UnexpectedDataException("query", query);
                throw new UnexpectedDataException("No results for query", innerException);
            }

            return geoCodeResults.results.FirstOrDefault()?.geometry.location;
        }

        public async Task<GoogleApiTimeZone> TimeZone(GoogleApiLocation location)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var query = $"location={location.lat.ToString(CultureInfo.InvariantCulture)},{location.lng.ToString(CultureInfo.InvariantCulture)}&timestamp={timestamp}";
            var url = await BuildApiUrl(GoogleApiEndpointEnum.TimeZone, query);
            var responseMessage = await SendAsync(url);
            var json = await responseMessage.Content.ReadAsStringAsync();
            var timeZoneResult = System.Text.Json.JsonSerializer.Deserialize<GoogleApiTimeZone>(json);
            return timeZoneResult;
        }
    }
}