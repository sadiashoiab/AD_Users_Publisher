using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Interfaces;
using Azure_AD_Users_Publisher.Services.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Azure_AD_Users_Publisher.Services
{
    public class SalesforceMessageProcessor : IMessageProcessor
    {
        private readonly ILogger<SalesforceMessageProcessor> _logger;
        private readonly ITokenService _tokenService;
        private readonly IProgramDataService _programDataService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ISalesforceUserPublishService _salesforceUserPublishService;

        public SalesforceMessageProcessor(ILogger<SalesforceMessageProcessor> logger, ITokenService tokenService, IProgramDataService programDataService, ITimeZoneService timeZoneService, ISalesforceUserPublishService salesforceUserPublishService)
        {
            _logger = logger;
            _tokenService = tokenService;
            _programDataService = programDataService;
            _timeZoneService = timeZoneService;
            _salesforceUserPublishService = salesforceUserPublishService;
        }

        public async Task ProcessMessage(ISubscriptionClient receiver,
            Message message,
            CancellationToken cancellationToken)
        {
            var messageBody = Encoding.UTF8.GetString(message.Body);
            var user = System.Text.Json.JsonSerializer.Deserialize<SalesforceUser>(messageBody);

            var syncUser = await ShouldUserBeSyncedToSalesforce(user);
            if (syncUser)
            {
                _logger.LogInformation($"User with ID: {user.ExternalId} will be published to Salesforce.");

                var operatingSystemTask = GetUserOperatingSystem(user);
                var timeZoneTask = _timeZoneService.RetrieveTimeZone(user);

                await Task.WhenAll(operatingSystemTask, timeZoneTask);

                user.OperatingSystem = await operatingSystemTask;
                user.TimeZone = await timeZoneTask;

                await _salesforceUserPublishService.Publish(user);
            }

            await receiver.CompleteAsync(message.SystemProperties.LockToken);
        }

        private async Task<string> GetUserOperatingSystem(SalesforceUser user)
        {
            var parsed = int.TryParse(user.FranchiseNumber, out var userFranchiseNumber);
            if (parsed)
            {
                var clearCareFranchises = await RetrieveClearCareFranchiseData();
                if (clearCareFranchises.Any(franchiseNumber => franchiseNumber == userFranchiseNumber))
                {
                    return "ClearCare";
                }
            }

            return "N/A";
        }

        private async Task<bool> ShouldUserBeSyncedToSalesforce(SalesforceUser user)
        {
            var parsed = int.TryParse(user.FranchiseNumber, out var userFranchiseNumber);
            if (parsed)
            {
                var salesforceFranchises = await RetrieveSalesforceFranchiseData();
                if (salesforceFranchises.Any(franchiseNumber => franchiseNumber == userFranchiseNumber))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<int[]> RetrieveSalesforceFranchiseData()
        {
            var bearerToken = await _tokenService.RetrieveToken();
            var salesforceFranchises = await _programDataService.RetrieveFranchises(ProgramDataSources.Salesforce, bearerToken);
            return salesforceFranchises;
        }

        private async Task<int[]> RetrieveClearCareFranchiseData()
        {
            var bearerToken = await _tokenService.RetrieveToken();
            var clearCareFranchises = await _programDataService.RetrieveFranchises(ProgramDataSources.ClearCare, bearerToken);
            return clearCareFranchises;
        }
    }
}