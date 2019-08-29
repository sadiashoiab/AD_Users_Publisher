using System.Collections.Generic;

namespace Azure_AD_Users_Publisher.Services.Models
{
    public class GoogleApiGeoCodeRootObject
    {
        public IList<GoogleApiGeoCodeResult> results { get; set; }
    }
}