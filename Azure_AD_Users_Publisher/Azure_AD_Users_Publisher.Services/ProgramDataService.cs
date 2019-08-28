using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Exceptions;
using Azure_AD_Users_Publisher.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Azure_AD_Users_Publisher.Services
{
    public class ProgramDataService : IProgramDataService
    {
        private readonly string _programDataUrl;
        private readonly IHttpClientFactory _httpClientFactory;

        public ProgramDataService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _programDataUrl = configuration["ProgramDataUrl"];
            _httpClientFactory = httpClientFactory;
        }

        public async Task<int[]> RetrieveFranchises(ProgramDataSources source, string bearerToken)
        {
            var url = BuildApiUrl(source);
            var responseMessage = await SendAsync(url, bearerToken);
            var json = await responseMessage.Content.ReadAsStringAsync();
            var results = System.Text.Json.JsonSerializer.Deserialize<int[]>(json);
            return results;
        }

        private string BuildApiUrl(ProgramDataSources source)
        {
            switch (source)
            {
                case ProgramDataSources.Salesforce:
                case ProgramDataSources.ClearCare:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source));
            }

            return $"{_programDataUrl}/v1/{source.GetDescription()}/franchises";
        }

        private async Task<HttpResponseMessage> SendAsync(string url, string bearerToken)
        {
            var client = _httpClientFactory.CreateClient("ProgramDataHttpClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

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
    }
}
