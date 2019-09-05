using System.Diagnostics.CodeAnalysis;

namespace Azure_AD_Users_Publisher.Services.Models
{
    [ExcludeFromCodeCoverage]
    public class GoogleApiGeoCodeResult
    {
        public GoogleApiGeometry geometry { get; set; }
    }
}