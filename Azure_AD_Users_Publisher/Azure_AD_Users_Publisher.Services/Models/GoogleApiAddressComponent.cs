using System.Collections.Generic;

namespace Azure_AD_Users_Publisher.Services.Models
{
    public class GoogleApiAddressComponent
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public IList<string> types { get; set; }
    }
}