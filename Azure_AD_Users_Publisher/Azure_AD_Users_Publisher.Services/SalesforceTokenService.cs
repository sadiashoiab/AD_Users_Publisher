using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Exceptions;
using Azure_AD_Users_Publisher.Services.Interfaces;
using Azure_AD_Users_Publisher.Services.Models;
using Microsoft.Extensions.Configuration;

namespace Azure_AD_Users_Publisher.Services
{
    public class SalesforceTokenService : ITokenService
    {
        private readonly CacheControlHeaderValue _noCacheControlHeaderValue =
            new CacheControlHeaderValue {NoCache = true};

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        private readonly string _tokenUrl;

        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private string ContentBody { get; set; }

        public SalesforceTokenService(IHttpClientFactory httpClientFactory, IAzureKeyVaultService azureKeyVaultService, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _azureKeyVaultService = azureKeyVaultService;
            _tokenUrl = configuration["SalesforceTokenUrl"];
        }

        private async Task<string> GetClientBody()
        {
            // note: check if it has been set, if so just give the value, otherwise...
            if (string.IsNullOrWhiteSpace(ContentBody))
            {
                // note: technically we could have many callers... use a lightweight semaphore to only allow one at a time for setting
                await _semaphoreSlim.WaitAsync();
                try
                {
                    // note: checking again, because caller other than myself could, in theory, have set it while we were waiting for semaphore...
                    //       and if so, no need to get it again here
                    if (string.IsNullOrWhiteSpace(ContentBody))
                    {
                        var clientIdTask = _azureKeyVaultService.GetSecret("SalesforceTokenClientId");
                        var clientSecretTask = _azureKeyVaultService.GetSecret("SalesforceTokenClientSecret");
                        var clientUsernameTask = _azureKeyVaultService.GetSecret("SalesforceTokenUsername");
                        var clientPasswordTask = _azureKeyVaultService.GetSecret("SalesforceTokenPassword");
                        await Task.WhenAll(clientIdTask, clientSecretTask, clientUsernameTask, clientPasswordTask);

                        ContentBody = $"grant_type=password&client_id={await clientIdTask}&client_secret={await clientSecretTask}&username={await clientUsernameTask}&password={await clientPasswordTask}";
                    }
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }

            return ContentBody;
        }

        public async Task<string> RetrieveToken()
        {
            var client = _httpClientFactory.CreateClient("TokenApiHttpClient");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _tokenUrl);
            requestMessage.Headers.CacheControl = _noCacheControlHeaderValue;

            var contentBody = await GetClientBody();
            var content = new StringContent(contentBody);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            requestMessage.Content = content;

            var responseMessage = await client.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new UnexpectedStatusCodeException(responseMessage);
            }

            if (responseMessage.Content == null)
            {
                throw new UnexpectedDataException(nameof(responseMessage.Content));
            }

            var json = await responseMessage.Content.ReadAsStringAsync();
            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<SalesforceTokenResponse>(json);
            return tokenResponse.access_token;
        }
    }
}