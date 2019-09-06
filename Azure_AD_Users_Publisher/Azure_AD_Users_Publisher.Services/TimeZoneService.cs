﻿using System;
using System.Threading.Tasks;
using Azure_AD_Users_Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Azure_AD_Users_Publisher.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        private const string _cacheKeyPrefix = "_TimeZone_";

        private readonly ILogger<TimeZoneService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IGoogleApiService _googleApiService;
        private readonly int _franchiseTimeZoneCacheDurationInHours;

        public TimeZoneService(ILogger<TimeZoneService> logger, IMemoryCache memoryCache, IGoogleApiService googleApiService, IConfiguration configuration)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _googleApiService = googleApiService;
            _franchiseTimeZoneCacheDurationInHours = int.Parse(configuration["FranchiseTimeZoneCacheDurationInHours"]);
        }

        public async Task<string> RetrieveTimeZoneAndPopulateUsersCountryCode(AzureActiveDirectoryUser user)
        {
            var cacheKey = $"{_cacheKeyPrefix}{user.FranchiseNumber}";
            if (!_memoryCache.TryGetValue(cacheKey, out string timeZone))
            {
                timeZone = await GetSalesforceSupportedTimeZoneAndPopulateUsersCountryCodeIfAvailable(user);

                if (!string.IsNullOrWhiteSpace(timeZone))
                {
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(_franchiseTimeZoneCacheDurationInHours),
                    };

                    _memoryCache.Set(cacheKey, timeZone, cacheOptions);
                }
            }

            return timeZone;
        }

        private async Task<string> GetSalesforceSupportedTimeZoneAndPopulateUsersCountryCodeIfAvailable(AzureActiveDirectoryUser user)
        {
            var geoCodeResult = await _googleApiService.GeoCode(user.Address, user.City, user.State, user.PostalCode);

            if (string.IsNullOrWhiteSpace(user.CountryCode))
            {
                user.CountryCode = geoCodeResult.ToCountryCode();
            }

            var timeZoneResult = await _googleApiService.TimeZone(geoCodeResult.geometry?.location);
            var timeZone = timeZoneResult.ToSalesforceTimeZone();
            if (string.IsNullOrWhiteSpace(timeZone))
            {
                _logger.LogWarning($"No Salesforce Supported Time Zone was found for dstOffset: {timeZoneResult.dstOffset}, rawOffset: {timeZoneResult.rawOffset}, timeZoneName: {timeZoneResult.timeZoneName}");
            }

            return timeZone;
        }
    }
}