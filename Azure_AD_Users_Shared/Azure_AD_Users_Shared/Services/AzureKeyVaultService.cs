using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;

namespace Azure_AD_Users_Shared.Services
{
    public class AzureKeyVaultService : IAzureKeyVaultService
    {
        private readonly string _url;

        public AzureKeyVaultService(IConfiguration configuration)
        {
            _url = configuration["KeyVaultUrl"];
        }

        // todo: look to swap this out and use the Azure Key Vault Configuration Provider that is in ASP.NET Core 

        // todo: this method is not testable due to usage of requiring a valid AzureServiceTokenProvider, refactor later.
        public async Task<string> GetSecret(string name)
        {
            var azureTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureTokenProvider.KeyVaultTokenCallback));
            var secret = await keyVaultClient.GetSecretAsync($"{_url}{name}");
            return secret.Value;
        }
    }
}
