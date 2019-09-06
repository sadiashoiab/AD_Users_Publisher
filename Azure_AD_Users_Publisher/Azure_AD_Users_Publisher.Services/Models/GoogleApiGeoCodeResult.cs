using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Azure_AD_Users_Publisher.Services.Models
{
    [ExcludeFromCodeCoverage]
    public class GoogleApiGeoCodeResult
    {
        public IList<GoogleApiAddressComponent> address_components { get; set; }
        public GoogleApiGeometry geometry { get; set; }
    }
}