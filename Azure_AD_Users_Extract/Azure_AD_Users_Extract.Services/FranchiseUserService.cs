using System.Collections.Generic;
using System.Threading.Tasks;
using Azure_AD_Users_Shared.Models;
using Microsoft.Extensions.Logging;

namespace Azure_AD_Users_Extract.Services
{
    public class FranchiseUserService : IFranchiseUserService
    {
        private readonly ILogger<FranchiseUserService> _logger;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;

        public FranchiseUserService(ILogger<FranchiseUserService> logger, ITokenService tokenService, IUserService userService)
        {
            _logger = logger;
            _tokenService = tokenService;
            _userService = userService;
        }

        public async Task<List<AzureActiveDirectoryUser>> GetFranchiseUsers(string groupId, int syncDurationInHours = 0)
        {
            var token = await _tokenService.RetrieveToken(TokenEnum.Franchise);
            var users = await _userService.GetUsers(groupId, token, syncDurationInHours);

            if (users?.Count > 0)
            {
                _logger.LogInformation($"{users.Count} Franchise users were retrieved");
            }

            return users;
        }
    }
}