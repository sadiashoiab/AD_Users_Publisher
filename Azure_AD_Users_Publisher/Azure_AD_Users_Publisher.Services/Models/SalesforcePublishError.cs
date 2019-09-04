using System.Diagnostics.CodeAnalysis;

namespace Azure_AD_Users_Publisher.Services.Models
{
    [ExcludeFromCodeCoverage]
    public class SalesforcePublishError
    {
        public bool hasError { get; set; }
        public string errorMessage { get; set; }
    }
}