using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;
using Azure_AD_Users_Shared.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Azure_AD_Users_Publisher.Services
{
    public class AzureLogicEmailService : IAzureLogicEmailService
    {
        private readonly CacheControlHeaderValue _noCacheControlHeaderValue = new CacheControlHeaderValue {NoCache = true};

        private readonly string _logicEmailUrl;
        private readonly IHttpClientFactory _httpClientFactory;

        public AzureLogicEmailService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logicEmailUrl = configuration["LogicEmailUrl"];
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendAlert(string message)
        {
            var client = _httpClientFactory.CreateClient("EmailAlertHttpClient");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _logicEmailUrl);
            requestMessage.Headers.CacheControl = _noCacheControlHeaderValue;

            var alert = new EmailAlert {alert = message};
            var json = System.Text.Json.JsonSerializer.Serialize(alert);
            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestMessage.Content = content;
            var responseMessage = await client.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new UnexpectedStatusCodeException(responseMessage);
            }
        }
    }
}