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
            var syncUserToSalesforce = await CheckUserFranchiseAgainstFranchiseSource(user, ProgramDataSources.Salesforce, true, false);
            if (syncUserToSalesforce)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(user);
                if (user.DeactivationDateTimeOffset.HasValue)
                {
                    _logger.LogInformation($"User will be Deactivated: {json}");
                    // todo: disabling for testing
                    //await _salesforceUserPublishService.Deactivate(user.ExternalId);
                }
                else
                {
                    var operatingSystemTask = CheckUserFranchiseAgainstFranchiseSource(user, ProgramDataSources.ClearCare, "ClearCare", "N/A");
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
                        Title = user.Title,
                        OperatingSystem = await operatingSystemTask,
                        TimeZone = await timeZoneTask
                    };

                    _logger.LogInformation($"User will be Published: {json}");
                    // todo: disabling for testing
                    //await _salesforceUserPublishService.Publish(salesforceUser);
                }

                _logger.LogInformation($"Publish Count: {_salesforceUserPublishService.PublishCount}, Deactivation Count: {_salesforceUserPublishService.DeactivationCount}, Error Count: {_salesforceUserPublishService.ErrorCount}");
            }
        }

        private async Task<T> CheckUserFranchiseAgainstFranchiseSource<T>(AzureActiveDirectoryUser user, ProgramDataSources source, T success, T fail)
        {
            var parsed = int.TryParse(user.FranchiseNumber, out var userFranchiseNumber);
            if (parsed)
            {
                var franchises = await RetrieveFranchiseData(source);
                if (franchises.Any(franchiseNumber => franchiseNumber == userFranchiseNumber))
                {
                    return success;
                }
            }

            return fail;
        }

        private async Task<int[]> RetrieveFranchiseData(ProgramDataSources source)
        {
            try
            {
                var bearerToken = await _tokenService.RetrieveToken();
                var franchises =  await _programDataService.RetrieveFranchises(source, bearerToken);
                return franchises;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An Exception occurred when trying to retrieve {source.GetDescription()} Franchise. StackTrace: {ex.StackTrace}");
            }

            return new int[] { };
        }
    }
}