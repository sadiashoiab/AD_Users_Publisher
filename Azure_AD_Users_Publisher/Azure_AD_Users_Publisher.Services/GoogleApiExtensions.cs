using System.Collections.Generic;
using System.Linq;
using Azure_AD_Users_Publisher.Services.Models;

namespace Azure_AD_Users_Publisher.Services
{
    public static class GoogleApiExtensions
    {
        //private static readonly IReadOnlyList<SalesforceSupportedTimeZone> _salesforceSupportedTimeZones = new List<SalesforceSupportedTimeZone>
        //{
        //    new SalesforceSupportedTimeZone(50400, "Line Is. Time", "Pacific/Kiritimati"),
        //    new SalesforceSupportedTimeZone(46800, "Phoenix Is.Time", "Pacific/Enderbury"),
        //    new SalesforceSupportedTimeZone(46800, "Tonga Time", "Pacific/Tongatapu"),
        //    new SalesforceSupportedTimeZone(45900, "Chatham Standard Time", "Pacific/Chatham"),
        //    new SalesforceSupportedTimeZone(43200, "New Zealand Standard Time", "Pacific/Auckland"),
        //    new SalesforceSupportedTimeZone(43200, "Fiji Time", "Pacific/Fiji"),
        //    new SalesforceSupportedTimeZone(43200, "Petropavlovsk-Kamchatski Time", "Asia/Kamchatka"),
        //    new SalesforceSupportedTimeZone(41400, "Norfolk Time", "Pacific/Norfolk"),
        //    new SalesforceSupportedTimeZone(39600, "Lord Howe Standard Time", "Australia/Lord_Howe"),
        //    new SalesforceSupportedTimeZone(39600, "Solomon Is. Time", "Pacific/Guadalcanal"),
        //    new SalesforceSupportedTimeZone(37800, "Australian Central Standard Time [South Australia]", "Australia/Adelaide"),
        //    new SalesforceSupportedTimeZone(36000, "Australian Eastern Standard Time [New South Wales]", "Australia/Sydney"),
        //    new SalesforceSupportedTimeZone(36000, "Australian Eastern Standard Time [Queensland]", "Australia/Brisbane"),
        //    new SalesforceSupportedTimeZone(34200, "Australian Central Standard Time [Northern Territory]", "Australia/Darwin"),
        //    new SalesforceSupportedTimeZone(32400, "Korea Standard Time", "Asia/Seoul"),
        //    new SalesforceSupportedTimeZone(32400, "Japan Standard Time", "Asia/Tokyo"),
        //    new SalesforceSupportedTimeZone(28800, "Hong Kong Time", "Asia/Hong_Kong"),
        //    new SalesforceSupportedTimeZone(28800, "Malaysia Time", "Asia/Kuala_Lumpur"),
        //    new SalesforceSupportedTimeZone(28800, "Philippines Time", "Asia/Manila"),
        //    new SalesforceSupportedTimeZone(28800, "China Standard Time", "Asia/Shanghai"),
        //    new SalesforceSupportedTimeZone(28800, "Singapore Time", "Asia/Singapore"),
        //    new SalesforceSupportedTimeZone(28800, "China Standard Time", "Asia/Taipei"),
        //    new SalesforceSupportedTimeZone(28800, "Australian Western Standard Time", "Australia/Perth"),
        //    new SalesforceSupportedTimeZone(25200, "Indochina Time", "Asia/Bangkok"),
        //    new SalesforceSupportedTimeZone(25200, "Indochina Time", "Asia/Ho_Chi_Minh"),
        //    new SalesforceSupportedTimeZone(25200, "West Indonesia Time", "Asia/Jakarta"),
        //    new SalesforceSupportedTimeZone(23400, "Myanmar Time", "Asia/Rangoon"),
        //    new SalesforceSupportedTimeZone(21600, "Bangladesh Time", "Asia/Dhaka"),
        //    new SalesforceSupportedTimeZone(20700, "Nepal Time", "Asia/Kathmandu"),
        //    new SalesforceSupportedTimeZone(19800, "India Standard Time", "Asia/Colombo"),
        //    new SalesforceSupportedTimeZone(19800, "India Standard Time", "Asia/Kolkata"),
        //    new SalesforceSupportedTimeZone(18000, "Pakistan Time", "Asia/Karachi"),
        //    new SalesforceSupportedTimeZone(18000, "Uzbekistan Time", "Asia/Tashkent"),
        //    new SalesforceSupportedTimeZone(18000, "Yekaterinburg Time", "Asia/Yekaterinburg"),
        //    new SalesforceSupportedTimeZone(16200, "Afghanistan Time", "Asia/Kabul"),
        //    new SalesforceSupportedTimeZone(14400, "Azerbaijan Summer Time", "Asia/Baku"),
        //    new SalesforceSupportedTimeZone(14400, "Gulf Standard Time", "Asia/Dubai"),
        //    new SalesforceSupportedTimeZone(14400, "Georgia Time", "Asia/Tbilisi"),
        //    new SalesforceSupportedTimeZone(14400, "Armenia Time", "Asia/Yerevan"),
        //    new SalesforceSupportedTimeZone(12600, "Iran Daylight Time", "Asia/Tehran"),
        //    new SalesforceSupportedTimeZone(10800, "East African Time", "Africa/Nairobi"),
        //    new SalesforceSupportedTimeZone(10800, "Arabia Standard Time", "Asia/Baghdad"),
        //    new SalesforceSupportedTimeZone(10800, "Arabia Standard Time", "Asia/Kuwait"),
        //    new SalesforceSupportedTimeZone(10800, "Arabia Standard Time", "Asia/Riyadh"),
        //    new SalesforceSupportedTimeZone(10800, "Moscow Standard Time", "Europe/Minsk"),
        //    new SalesforceSupportedTimeZone(10800, "Moscow Standard Time", "Europe/Moscow"),
        //    new SalesforceSupportedTimeZone(10800, "Eastern European Summer Time", "Africa/Cairo"),
        //    new SalesforceSupportedTimeZone(10800, "Eastern European Summer Time", "Asia/Beirut"),
        //    new SalesforceSupportedTimeZone(10800, "Israel Daylight Time", "Asia/Jerusalem"),
        //    new SalesforceSupportedTimeZone(10800, "Eastern European Summer Time", "Europe/Athens"),
        //    new SalesforceSupportedTimeZone(10800, "Eastern European Summer Time", "Europe/Bucharest"),
        //    new SalesforceSupportedTimeZone(10800, "Eastern European Summer Time", "Europe/Helsinki"),
        //    new SalesforceSupportedTimeZone(10800, "Eastern European Summer Time", "Europe/Istanbul"),
        //    new SalesforceSupportedTimeZone(7200, "South Africa Standard Time", "Africa/Johannesburg"),
        //    new SalesforceSupportedTimeZone(7200, "Central European Summer Time", "Europe/Amsterdam"),
        //    new SalesforceSupportedTimeZone(7200, "Central European Summer Time", "Europe/Berlin"),
        //    new SalesforceSupportedTimeZone(7200, "Central European Summer Time", "Europe/Brussels"),
        //    new SalesforceSupportedTimeZone(7200, "Central European Summer Time", "Europe/Paris"),
        //    new SalesforceSupportedTimeZone(7200, "Central European Summer Time", "Europe/Prague"),
        //    new SalesforceSupportedTimeZone(7200, "Central European Summer Time", "Europe/Rome"),
        //    new SalesforceSupportedTimeZone(3600, "Western European Summer Time", "Europe/Lisbon"),
        //    new SalesforceSupportedTimeZone(3600, "Central European Time", "Africa/Algiers"),
        //    new SalesforceSupportedTimeZone(3600, "British Summer Time", "Europe/London"),
        //    new SalesforceSupportedTimeZone(-3600, "Cape Verde Time", "Atlantic/Cape_Verde"),
        //    new SalesforceSupportedTimeZone(0, "Western European Time", "Africa/Casablanca"),
        //    new SalesforceSupportedTimeZone(0, "Irish Summer Time", "Europe/Dublin"),
        //    new SalesforceSupportedTimeZone(0, "Greenwich Mean Time", "GMT"),
        //    new SalesforceSupportedTimeZone(0, "Eastern Greenland Summer Time", "America/Scoresbysund"),
        //    new SalesforceSupportedTimeZone(0, "Azores Summer Time", "Atlantic/Azores"),
        //    new SalesforceSupportedTimeZone(-7200, "South Georgia Standard Time", "Atlantic/South_Georgia"),
        //    new SalesforceSupportedTimeZone(-9000, "Newfoundland Daylight Time", "America/St_Johns"),
        //    new SalesforceSupportedTimeZone(-10800, "Brasilia Summer Time", "America/Sao_Paulo"),
        //    new SalesforceSupportedTimeZone(-10800, "Argentina Time", "America/Argentina/Buenos_Aires"),
        //    new SalesforceSupportedTimeZone(-10800, "Chile Summer Time", "America/Santiago"),
        //    new SalesforceSupportedTimeZone(-10800, "Atlantic Daylight Time", "America/Halifax"),
        //    new SalesforceSupportedTimeZone(-14400, "Atlantic Standard Time", "America/Puerto_Rico"),
        //    new SalesforceSupportedTimeZone(-14400, "Atlantic Daylight Time", "Atlantic/Bermuda"),
        //    new SalesforceSupportedTimeZone(-16200, "Venezuela Time", "America/Caracas"),
        //    //new SalesforceSupportedTimeZone(-14400, "Eastern Daylight Time", "America/Indiana/Indianapolis"),
        //    new SalesforceSupportedTimeZone(-14400, "Eastern Daylight Time", "America/New_York"),
        //    new SalesforceSupportedTimeZone(-18000, "Colombia Time", "America/Bogota"),
        //    new SalesforceSupportedTimeZone(-18000, "Peru Time", "America/Lima"),
        //    new SalesforceSupportedTimeZone(-18000, "Eastern Standard Time", "America/Panama"),
        //    //new SalesforceSupportedTimeZone(-18000, "Central Daylight Time", "America/Mexico_City"),
        //    new SalesforceSupportedTimeZone(-18000, "Central Daylight Time", "America/Chicago"),
        //    new SalesforceSupportedTimeZone(-21600, "Central Standard Time", "America/El_Salvador"),
        //    new SalesforceSupportedTimeZone(-21600, "Mountain Daylight Time", "America/Denver"),
        //    new SalesforceSupportedTimeZone(-21600, "Mountain Standard Time", "America/Mazatlan"),
        //    new SalesforceSupportedTimeZone(-25200, "Mountain Standard Time", "America/Phoenix"),
        //    new SalesforceSupportedTimeZone(-25200, "Pacific Daylight Time", "America/Los_Angeles"),
        //    //new SalesforceSupportedTimeZone(-25200, "Pacific Daylight Time", "America/Tijuana"),
        //    new SalesforceSupportedTimeZone(-28800, "Pitcairn Standard Time", "Pacific/Pitcairn"),
        //    new SalesforceSupportedTimeZone(-28800, "Alaska Daylight Time", "America/Anchorage"),
        //    new SalesforceSupportedTimeZone(-32400, "Gambier Time", "Pacific/Gambier"),
        //    new SalesforceSupportedTimeZone(-32400, "Hawaii-Aleutian Standard Time", "America/Adak"),
        //    new SalesforceSupportedTimeZone(-34200, "Marquesas Time", "Pacific/Marquesas"),
        //    new SalesforceSupportedTimeZone(-36000, "Hawaii-Aleutian Standard Time", "Pacific/Honolulu"),
        //    new SalesforceSupportedTimeZone(-39600, "Niue Time", "Pacific/Niue"),
        //    new SalesforceSupportedTimeZone(-39600, "Samoa Standard Time", "Pacific/Pago_Pago")
        //};

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