using System.Diagnostics.CodeAnalysis;

namespace Azure_AD_Users_Publisher.Services.Models
{
    [ExcludeFromCodeCoverage]
    public class GoogleApiTimeZoneResult
    {
        //{
        //    "dstOffset" : 3600,
        //    "rawOffset" : -18000,
        //    "status" : "OK",
        //    "timeZoneId" : "America/Toronto",
        //    "timeZoneName" : "Eastern Daylight Time"
        //}
        public int dstOffset { get; set; }
        public int rawOffset { get; set; }
        public string status { get; set; }
        public string timeZoneId { get; set; }
        public string timeZoneName { get; set; }
    }
}