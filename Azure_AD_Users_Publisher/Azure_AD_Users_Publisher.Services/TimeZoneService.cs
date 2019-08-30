using System;
using System.Threading.Tasks;
using Azure_AD_Users_Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Azure_AD_Users_Publisher.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        private const string _cacheKeyPrefix = "_TimeZone_";

        private readonly IMemoryCache _memoryCache;
        private readonly IGoogleApiService _googleApiService;
        private readonly int _franchiseTimeZoneCacheDurationInHours;

        public TimeZoneService(IMemoryCache memoryCache, IGoogleApiService googleApiService, IConfiguration configuration)
        {
            _memoryCache = memoryCache;
            _googleApiService = googleApiService;
            _franchiseTimeZoneCacheDurationInHours = int.Parse(configuration["FranchiseTimeZoneCacheDurationInHours"]);
        }

        // todo: refactor all services that cache to have a common CachingService
        //       should take a cacheKeyPrefix, Func, MemoryCacheOptions?
        public async Task<string> RetrieveTimeZone(SalesforceUser user)
        {
            var cacheKey = $"{_cacheKeyPrefix}{user.FranchiseNumber}";
            if (!_memoryCache.TryGetValue(cacheKey, out string timeZone))
            {
                var googleLocation = await _googleApiService.GeoCode(user.Address, user.City, user.State, user.PostalCode);
                timeZone = await _googleApiService.TimeZone(googleLocation);

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(_franchiseTimeZoneCacheDurationInHours),
                };

                _memoryCache.Set(cacheKey, timeZone, cacheOptions);
            }

            return timeZone;
        }
    }
}