using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Azure_AD_Users_Publisher.Services.Models
{
    [ExcludeFromCodeCoverage]
    public class GoogleApiGeoCodeRootObject
    {
        public IList<GoogleApiGeoCodeResult> results { get; set; }
    }
}