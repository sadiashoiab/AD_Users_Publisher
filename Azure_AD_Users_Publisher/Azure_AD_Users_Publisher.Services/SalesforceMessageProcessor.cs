using System;
using System.Linq;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;
using Azure_AD_Users_Shared.Models;
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

        public async Task ProcessUser(AzureActiveDirectoryUser user)
        {
            var syncUserToSalesforce = await ShouldUserBeSyncedToSalesforce(user);
            if (syncUserToSalesforce)
            {
                // todo: remove once we have approval we can start hitting the service automatically
                //if (user.DeactivationDateTimeOffset.HasValue)
                //{
                //    _logger.LogInformation($"User with ID: {user.ExternalId} will be Deactivated.");
                //    await _salesforceUserPublishService.DeactivateUser(user);
                //}
                //else
                //{
                _logger.LogInformation($"User with ID: {user.ExternalId} will be Published.");

                var operatingSystemTask = GetUserOperatingSystem(user);
                var timeZoneTask = _timeZoneService.RetrieveTimeZoneAndPopulateUsersCountryCode(user);

                await Task.WhenAll(operatingSystemTask, timeZoneTask);

                var salesforceUser = new SalesforceUser
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    FranchiseNumber = user.FranchiseNumber,
                    ExternalId = user.ExternalId,
                    FederationId = user.FederationId,
                    MobilePhone = user.MobilePhone,
                    Address = user.Address,
                    City = user.City,
                    State = user.State,
                    PostalCode = user.PostalCode,
                    CountryCode = user.CountryCode,
                    IsOwner = user.IsOwner,
                    OperatingSystem = await operatingSystemTask,
                    TimeZone = await timeZoneTask
                };

                await _salesforceUserPublishService.Publish(salesforceUser);
                //}
            }
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