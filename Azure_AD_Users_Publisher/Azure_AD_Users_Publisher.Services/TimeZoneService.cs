using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure_AD_Users_Shared.Models;
using LazyCache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Azure_AD_Users_Publisher.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        private const string _cacheKeyPrefix = "_TimeZone_";

        private readonly ILogger<TimeZoneService> _logger;
        private readonly IAppCache _cache;
        private readonly IGoogleApiService _googleApiService;
        private readonly ISalesforceTimeZoneManagerService _salesforceTimeZoneManagerService;
        private readonly int _franchiseTimeZoneCacheDurationInHours;

        public TimeZoneService(ILogger<TimeZoneService> logger, IAppCache cache, IGoogleApiService googleApiService, IConfiguration configuration, ISalesforceTimeZoneManagerService salesforceTimeZoneManagerService)
        {
            _logger = logger;
            _cache = cache;
            _googleApiService = googleApiService;
            _salesforceTimeZoneManagerService = salesforceTimeZoneManagerService;
            _franchiseTimeZoneCacheDurationInHours = int.Parse(configuration["FranchiseTimeZoneCacheDurationInHours"]);
        }

        private async Task<(string TimeZone, string Country)> GetUserTimeZoneCountry(AzureActiveDirectoryUser user)
        {
            var timeZone = await GetSalesforceSupportedTimeZoneAndPopulateUsersCountryCodeIfAvailable(user);
            return (timeZone, user.CountryCode);
        }

        public async Task<string> RetrieveTimeZoneAndPopulateUsersCountryCode(AzureActiveDirectoryUser user)
        {
            var cacheKey = $"{_cacheKeyPrefix}{user.FranchiseNumber}";
            var timeZoneAndCountry = await _cache.GetOrAddAsync(cacheKey, () => GetUserTimeZoneCountry(user), DateTimeOffset.UtcNow.AddHours(_franchiseTimeZoneCacheDurationInHours));
            user.CountryCode = timeZoneAndCountry.Country;
            return timeZoneAndCountry.TimeZone;
        }

        private async Task<string> GetSalesforceSupportedTimeZoneAndPopulateUsersCountryCodeIfAvailable(AzureActiveDirectoryUser user)
        {
            var geoCodeResult = await _googleApiService.GeoCode(user.Address, user.City, user.State, user.PostalCode);

            if (string.IsNullOrWhiteSpace(user.CountryCode))
            {
                user.CountryCode = geoCodeResult.ToCountryCode();
            }

            var timeZoneResult = await _googleApiService.TimeZone(geoCodeResult.geometry?.location);
            var salesforceSupportedTimeZones = await _salesforceTimeZoneManagerService.GetSupportedTimeZones();
            var timeZone = timeZoneResult.ToSalesforceTimeZone(salesforceSupportedTimeZones);
            if (string.IsNullOrWhiteSpace(timeZone))
            {
                var userJson = System.Text.Json.JsonSerializer.Serialize(user);
                var salesforceTimeZonesJson = System.Text.Json.JsonSerializer.Serialize(salesforceSupportedTimeZones);
                _logger.LogWarning($"No Salesforce Supported Time Zone was found for dstOffset: {timeZoneResult.dstOffset}, rawOffset: {timeZoneResult.rawOffset}, timeZoneName: {timeZoneResult.timeZoneName}, for user: {userJson}, with salesforceTimeZones: {salesforceTimeZonesJson}");
            }

            return timeZone;
        }

        [ExcludeFromCodeCoverage]
        public class CacheTimeZoneCountry
        {
            public string TimeZone { get; set; }
            public string Country { get; set; }
        }
    }
}