using System.Collections.Generic;
using System.Linq;
using Azure_AD_Users_Publisher.Services.Models;

namespace Azure_AD_Users_Publisher.Services
{
    public static class GoogleApiExtensions
    {
        public static string ToSalesforceTimeZone(this GoogleApiTimeZoneResult googleTimeZoneResult, IList<SalesforceSupportedTimeZone> salesforceSupportedTimeZones)
        {
            var timeZoneOffset = googleTimeZoneResult.rawOffset + googleTimeZoneResult.dstOffset;
            var matches = salesforceSupportedTimeZones.Where(timeZone => timeZone.TimeZoneOffset == timeZoneOffset).ToList();

            if (matches.Count == 1)
            {
                return matches.First().TimeZoneId;
            }

            if (matches.Any())
            {
                var timeZoneNameMatches = matches.Where(match => match.TimeZoneName.Equals(googleTimeZoneResult.timeZoneName)).ToList();

                if (timeZoneNameMatches.Any())
                {
                    return timeZoneNameMatches.FirstOrDefault().TimeZoneId;
                }
            }

            return string.Empty;
        }

        public static string ToCountryCode(this GoogleApiGeoCodeResult googleApiGeoCodeResult)
        {
            var countryCode = googleApiGeoCodeResult?.address_components?.FirstOrDefault(component => component.types.Contains("country"))?.short_name;
            return countryCode;
        }
    }
}