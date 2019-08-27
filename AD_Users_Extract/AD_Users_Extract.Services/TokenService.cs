using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AD_Users_Extract.Services.Interfaces;

namespace AD_Users_Extract.Services
{
    public class TokenService : ITokenService
    {
        private readonly IAzureKeyVaultService _keyVaultService;
        private readonly IGraphApiService _graphApiService;

        public TokenService(IAzureKeyVaultService keyVaultService, IGraphApiService graphApiService)
        {
            _keyVaultService = keyVaultService;
            _graphApiService = graphApiService;
        }

        public async Task<string> RetrieveToken(TokenEnum tokenEnum)
        {
            var (clientIdName, appKeyName, tenantName) = GetSecretNames(tokenEnum);
            var (clientId, appKey, tenant) = await GetSecretValues(clientIdName, appKeyName, tenantName);

            return await _graphApiService.AcquireToken(clientId, appKey, tenant);
        }

        private (string clientIdName, string appKeyName, string tenantName) GetSecretNames(TokenEnum tokenEnum)
        {
            string prefix;

            switch (tokenEnum)
            {
                case TokenEnum.Franchise:
                    prefix = "NA"; 
                    break;
                case TokenEnum.HomeOffice:
                    prefix = "HO"; 
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tokenEnum), $"Provided tokenEnum ({tokenEnum.ToString()}) is invalid.");
            }

            var clientIdName = $"{prefix}ClientId";
            var appKeyName = $"{prefix}AppKey";
            var tenantName = $"{prefix}Tenant";

            return (clientIdName, appKeyName, tenantName);
        }

        private async Task<(string clientId, string appKey, string tenant)> GetSecretValues(string clientIdName, string appKeyName, string tenantName)
        {
            var tasks = new List<Task<string>>
            {
                _keyVaultService.GetSecret(clientIdName),
                _keyVaultService.GetSecret(appKeyName),
                _keyVaultService.GetSecret(tenantName)
            };

            var results = await Task.WhenAll(tasks);
            return (results[0], results[1], results[2]);
        }
    }
}
