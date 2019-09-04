using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure_AD_Users_Shared.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Azure_AD_Users_Publisher.Services
{
    public class SalesforceMessageProcessor : IMessageProcessor
    {
        private readonly ILogger<SalesforceMessageProcessor> _logger;
        private readonly IHISCTokenService _tokenService;
        private readonly IProgramDataService _programDataService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ISalesforceUserPublishService _salesforceUserPublishService;

        public SalesforceMessageProcessor(ILogger<SalesforceMessageProcessor> logger, IHISCTokenService tokenService, IProgramDataService programDataService, ITimeZoneService timeZoneService, ISalesforceUserPublishService salesforceUserPublishService)
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
            var user = System.Text.Json.JsonSerializer.Deserialize<AzureActiveDirectoryUser>(messageBody);

            var syncUserToSalesforce = await ShouldUserBeSyncedToSalesforce(user);
            if (syncUserToSalesforce)
            {
                await ProcessSalesforceUser(user);
            }

            await receiver.CompleteAsync(GetLockToken(message));
        }

        private async Task ProcessSalesforceUser(AzureActiveDirectoryUser user)
        {
            if (user.DeactivationDateTimeOffset.HasValue)
            {
                _logger.LogInformation($"User with ID: {user.ExternalId} will be Deactivated.");
                // todo: remove once we have approval we can start hitting the service automatically
                //await _salesforceUserPublishService.DeactivateUser(user);
            }
            else
            {
                _logger.LogInformation($"User with ID: {user.ExternalId} will be Published.");

                var operatingSystemTask = GetUserOperatingSystem(user);
                var timeZoneTask = _timeZoneService.RetrieveTimeZone(user);

                await Task.WhenAll(operatingSystemTask, timeZoneTask);

                user.OperatingSystem = await operatingSystemTask;
                user.TimeZone = await timeZoneTask;

                // todo: remove this after salesforce endpoint has been modified to accept the State coming from Azure AD
                user.State = "NE";

                // todo: remove once we have approval we can start hitting the service automatically
                //await _salesforceUserPublishService.Publish(user);
            }
        }

        private string GetLockToken(Message message)
        {
            return message.SystemProperties.IsLockTokenSet ? message.SystemProperties.LockToken : null;
        }

        private async Task<string> GetUserOperatingSystem(AzureActiveDirectoryUser user)
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

        private async Task<bool> ShouldUserBeSyncedToSalesforce(AzureActiveDirectoryUser user)
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
            try
            {
                var bearerToken = await _tokenService.RetrieveToken();
                var salesforceFranchises = await _programDataService.RetrieveFranchises(ProgramDataSources.Salesforce, bearerToken);
                return salesforceFranchises;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An Exception occurred when trying to retrieve Salesforce Franchises. StackTrace: {ex.StackTrace}");
            }

            return new int[] { };
        }

        private async Task<int[]> RetrieveClearCareFranchiseData()
        {
            try
            {

                var bearerToken = await _tokenService.RetrieveToken();
                var clearCareFranchises =  await _programDataService.RetrieveFranchises(ProgramDataSources.ClearCare, bearerToken);
                return clearCareFranchises;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An Exception occurred when trying to retrieve ClearCare Franchise. StackTrace: {ex.StackTrace}");
            }

            return new int[] { };
        }
    }
}