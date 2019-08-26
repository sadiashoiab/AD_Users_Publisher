﻿using System;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using AD_Users_Extract.Services.Exceptions;
using AD_Users_Extract.Services.Interfaces;
using AD_Users_Extract.Services.Models;

namespace AD_Users_Extract.Services
{
    public class GoogleService : IGoogleService
    {
        private const string _googleApiBaseUrl = "https://maps.googleapis.com/maps/api/";
        
        private readonly IAzureKeyVaultService _keyVaultService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UrlEncoder _urlEncoder = UrlEncoder.Default;

        private string GoogleApiKey { get; set; }

        public GoogleService(IAzureKeyVaultService keyVaultService, IHttpClientFactory httpClientFactory)
        {
            _keyVaultService = keyVaultService;
            _httpClientFactory = httpClientFactory;
        }

        private async Task<string> BuildApiUrl(GoogleApiEndpoint googleApiEndpoint, string query)
        {
            switch (googleApiEndpoint)
            {
                case GoogleApiEndpoint.GeoCode:
                case GoogleApiEndpoint.TimeZone:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(googleApiEndpoint));
            }

            if (string.IsNullOrWhiteSpace(GoogleApiKey))
            {
                GoogleApiKey = await _keyVaultService.GetSecret("GoogleApiKey");
            }

            return $"{_googleApiBaseUrl}{googleApiEndpoint.ToString().ToLower()}/json?{query}&key={GoogleApiKey}";
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
            var url = await BuildApiUrl(GoogleApiEndpoint.GeoCode, query);

            var responseMessage = await SendAsync(url);
            var json = await responseMessage.Content.ReadAsStringAsync();
            var geoCodeResults = JsonSerializer.Deserialize<GoogleApiGeoCodeRootObject>(json);
            if (!geoCodeResults.results.Any())
            {
                var innerException = new UnexpectedDataException("query", query);
                throw new UnexpectedDataException("No results for query", innerException);
            }

            return geoCodeResults.results.FirstOrDefault()?.geometry.location;
        }

        public Task<string> TimeZone(string latitude, string longitude)
        {
            throw new NotImplementedException();
        }
    }

    public enum GoogleApiEndpoint
    {
        GeoCode,
        TimeZone
    }
}
