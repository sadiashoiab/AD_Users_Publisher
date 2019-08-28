using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Exceptions;
using Azure_AD_Users_Publisher.Services.Interfaces;

namespace Azure_AD_Users_Publisher.Services
{
    public class TokenService : ITokenService
    {
        private const string _tokenUri = "https://login.microsoftonline.com/HOMEINSTEADINC1.onmicrosoft.com/oauth2/token";
        
        private readonly CacheControlHeaderValue _noCacheControlHeaderValue = new CacheControlHeaderValue() {NoCache = true};
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        
        private string ContentBody { get; set; }

        public TokenService(IHttpClientFactory httpClientFactory, IAzureKeyVaultService azureKeyVaultService)
        {
            _httpClientFactory = httpClientFactory;
            _azureKeyVaultService = azureKeyVaultService;
        }

        private async Task<string> GetClientBody()
        {
            if (string.IsNullOrWhiteSpace(ContentBody))
            {
                var clientIdTask = _azureKeyVaultService.GetSecret("BearerTokenClientId");
                var clientSecretTask = _azureKeyVaultService.GetSecret("BearerTokenClientSecret");
                await Task.WhenAll(clientIdTask, clientSecretTask);
                ContentBody = $"client_id={await clientIdTask}&client_secret={await clientSecretTask}=&grant_type=client_credentials&resource=https://hiscprogramdatadv.azurewebsites.net";
            }

            return ContentBody;
        }

        public async Task<string> RetrieveToken()
        {
            var client = _httpClientFactory.CreateClient("TokenApiHttpClient");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _tokenUri);
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

            var token = await ParseAccessTokenFromResponseContent(responseMessage.Content);
            return token;
        }

        private async Task<string> ParseAccessTokenFromResponseContent(HttpContent httpContent)
        {
            var json = await httpContent.ReadAsStringAsync();
            using (var jsonDocument = System.Text.Json.JsonDocument.Parse(json))
            {
                var propertyExisted = jsonDocument.RootElement.TryGetProperty("access_token", out var accessTokenJsonElement);
                if (propertyExisted)
                {
                    var accessToken = accessTokenJsonElement.GetString();
                    return accessToken;
                }

                throw new UnexpectedDataException(nameof(accessTokenJsonElement));
            }
        }
    }
}