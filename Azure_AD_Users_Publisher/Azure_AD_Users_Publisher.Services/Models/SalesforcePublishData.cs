using System.Diagnostics.CodeAnalysis;

namespace Azure_AD_Users_Publisher.Services.Models
{
    [ExcludeFromCodeCoverage]
    public class SalesforcePublishData
    {
        public string userName { get; set; }
        public string successMessage { get; set; }
        public string recordId { get; set; }
    }
}