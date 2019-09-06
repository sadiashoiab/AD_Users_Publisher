using System;
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

        public async Task<string> RetrieveTimeZone(AzureActiveDirectoryUser user)
        {
            var cacheKey = $"{_cacheKeyPrefix}{user.FranchiseNumber}";
            if (!_memoryCache.TryGetValue(cacheKey, out string timeZone))
            {
                var googleLocation = await _googleApiService.GeoCode(user.Address, user.City, user.State, user.PostalCode);
                var result = await _googleApiService.TimeZone(googleLocation);
                timeZone = result.ToSalesforceTimeZone();

                if (!string.IsNullOrWhiteSpace(timeZone))
                {
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(_franchiseTimeZoneCacheDurationInHours),
                    };

                    _memoryCache.Set(cacheKey, timeZone, cacheOptions);
                }
                else
                {
                    _logger.LogWarning($"No Salesforce Supported Time Zone was found for dstOffset: {result.dstOffset}, rawOffset: {result.rawOffset}, timeZoneName: {result.timeZoneName}");
                }
            }

            return timeZone;
        }
    }
}