using System;
using System.Diagnostics.CodeAnalysis;

namespace Azure_AD_Users_Publisher.Services.Models
{
    [ExcludeFromCodeCoverage]
    public class HISCTokenResponse
    {
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string ext_expires_in { get; set; }
        public string expires_on { get; set; }
        public string not_before { get; set; }
        public string resource { get; set; }
        public string access_token { get; set; }

        public long expires_on_as_unix_time_seconds
        {
            get
            {
                var parsed = long.TryParse(expires_on, out var onSeconds);
                if (parsed)
                {
                    return onSeconds - 60;
                }

                // default to 60 seconds from now
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60;
            }
        }
    }
}