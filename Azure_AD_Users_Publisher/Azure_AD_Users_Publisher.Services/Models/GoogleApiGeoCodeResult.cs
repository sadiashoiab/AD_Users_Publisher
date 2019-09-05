using System.Diagnostics.CodeAnalysis;

namespace Azure_AD_Users_Publisher.Services.Models
{
    [ExcludeFromCodeCoverage]
    public class GoogleApiGeoCodeResult
    {
        public GoogleApiGeometry geometry { get; set; }
    }


    // todo: discuss with Steven if we need this or if he can ignore any properties that would be added now/future?
}