using System.Diagnostics.CodeAnalysis;

namespace Azure_AD_Users_Publisher.Services.Models
{
    [ExcludeFromCodeCoverage]
    public class SalesforcePublishResponse
    {
        public SalesforcePublishError error { get; set; }
        public SalesforcePublishData data { get; set; }
    }
}