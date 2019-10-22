using System;
using System.Linq;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Models;
using Azure_AD_Users_Shared.Models;
using LazyCache;
using Microsoft.Extensions.Logging;

namespace Azure_AD_Users_Publisher.Services
{
    public class SalesforceMessageProcessor : IMessageProcessor
    {
        private const string _cacheKeyPrefix = "_SalesforceMessageProcessor_";

        private readonly IAppCache _cache;
        private readonly ILogger<SalesforceMessageProcessor> _logger;
        private readonly IHISCTokenService _tokenService;
        private readonly IProgramDataService _programDataService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ISalesforceUserService _salesforceUserService;

        public SalesforceMessageProcessor(IAppCache cache, ILogger<SalesforceMessageProcessor> logger, IHISCTokenService tokenService, IProgramDataService programDataService, ITimeZoneService timeZoneService, ISalesforceUserService salesforceUserService)
        {
            _cache = cache;
            _logger = logger;
            _tokenService = tokenService;
            _programDataService = programDataService;
            _timeZoneService = timeZoneService;
            _salesforceUserService = salesforceUserService;
        }

        public async Task ProcessUser(AzureActiveDirectoryUser user)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(user);
            var syncUserToSalesforce = await CheckUserFranchiseAgainstFranchiseSource(user, ProgramDataSources.Salesforce, true, false);
            if (syncUserToSalesforce)
            {
                if (user.DeactivationDateTimeOffset.HasValue && await UserExistsAndIsActiveInSalesforce(user))
                {
                    _logger.LogInformation($"User will be Deactivated: {json}");
                    await _salesforceUserService.Deactivate(user.ExternalId);
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
                    await _salesforceUserService.Publish(salesforceUser);
                }

                _logger.LogInformation($"Publish Count: {_salesforceUserService.PublishCount}, Deactivation Count: {_salesforceUserService.DeactivationCount}, Error Count: {_salesforceUserService.ErrorCount}");
            }
            else
            {
                _logger.LogInformation($"User will NOT be sent to Salesforce: {json}");
            }
        }

        private async Task<bool> UserExistsAndIsActiveInSalesforce(AzureActiveDirectoryUser user)
        {
            // note: using the default caching duration of 20 minutes
            var allUsers = await _cache.GetOrAddAsync($"{_cacheKeyPrefix}AllUsers", _salesforceUserService.RetrieveAllUsers);
            var isActive = allUsers.records.Any(sfUser => sfUser.IsActive 
                                                  && sfUser.HI_GUID__c != null 
                                                  && sfUser.HI_GUID__c.Equals(user.ExternalId));
            if (!isActive)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(user);
                _logger.LogDebug($"User is NOT active OR does not exist in Salesforce: {json}");
            }

            return isActive;
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
                throw;
            }
        }
    }
}