using System.Collections.Generic;

namespace AD_Users_Extract.Services.Models
{
    public class GoogleApiGeoCodeRootObject
    {
        public IList<GoogleApiGeoCodeResult> results { get; set; }
    }
}
