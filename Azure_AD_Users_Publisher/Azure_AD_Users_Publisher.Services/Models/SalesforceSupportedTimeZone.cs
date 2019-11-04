using System.Diagnostics.CodeAnalysis;

namespace Azure_AD_Users_Publisher.Services.Models
{
    [ExcludeFromCodeCoverage]
    public struct SalesforceSupportedTimeZone
    {
        public SalesforceSupportedTimeZone(int gmtOffset, string timeZoneName, string timeZoneId)
        {
            TimeZoneOffset = gmtOffset;
            TimeZoneName = timeZoneName;
            TimeZoneId = timeZoneId;
        }

        public int TimeZoneOffset { get; }
        public string TimeZoneName { get; }
        public string TimeZoneId { get; }
    }
}