﻿using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;

namespace AD_Users_Publisher.Services
{
    public class AzureKeyVaultService : IAzureKeyVaultService
    {
        private readonly string _url;

        public AzureKeyVaultService(IConfiguration config)
        {
            _url = config["KeyVaultUrl"];
        }

        // todo: this method is not testable due to usage of requiring a valid AzureServiceTokenProvider, refactor later.
        public async Task<string> GetSecret(string name)
        {
            var azureTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureTokenProvider.KeyVaultTokenCallback));
            var secret = await keyVaultClient.GetSecretAsync(_url + name);
            return secret.Value;
        }
    }
}