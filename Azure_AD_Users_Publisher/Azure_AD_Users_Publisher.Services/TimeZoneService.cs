using System;
using System.Threading.Tasks;
using Azure_AD_Users_Shared.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Azure_AD_Users_Publisher.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        private const string _cacheKeyPrefix = "_TimeZone_";

        private readonly IMemoryCache _memoryCache;
        private readonly IGoogleApiService _googleApiService;

        public TimeZoneService(IMemoryCache memoryCache, IGoogleApiService googleApiService)
        {
            _memoryCache = memoryCache;
            _googleApiService = googleApiService;
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
                    // todo: move this to configuration later
                    // cache franchise's timezone for 18 hours by default
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(18),
                };

                _memoryCache.Set(cacheKey, timeZone, cacheOptions);
            }

            return timeZone;
        }
    }
}