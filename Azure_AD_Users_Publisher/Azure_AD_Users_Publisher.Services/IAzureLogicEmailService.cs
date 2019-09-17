using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure_AD_Users_Shared.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Azure_AD_Users_Publisher.Services
{
    public interface IAzureLogicEmailService
    {
        Task SendMessage(string message);
    }

    public class AzureLogicEmailService : IAzureLogicEmailService
    {
        private string _logicEmailUrl;
        private IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _tokenService;

        public AzureLogicEmailService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ITokenService tokenService)
        {
            _logicEmailUrl = configuration["LogicEmailUrl"];
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
        }
        public async Task SendMessage(string message)
        {
            var bearerToken = await _tokenService.RetrieveToken();
            await SendAsync(bearerToken, message);
        }

        private async Task SendAsync(string bearerToken, string message)
        {
            var client = _httpClientFactory.CreateClient("ProgramDataHttpClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _logicEmailUrl);
            var responseMessage = await client.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new UnexpectedStatusCodeException(responseMessage);
            }

            if (responseMessage.Content == null)
            {
                throw new UnexpectedDataException(nameof(responseMessage.Content));
            }
        }
    }
}