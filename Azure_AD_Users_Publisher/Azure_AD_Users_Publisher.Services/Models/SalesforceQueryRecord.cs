using System.Diagnostics.CodeAnalysis;

namespace Azure_AD_Users_Publisher.Services.Models
{
    [ExcludeFromCodeCoverage]
    public class SalesforceQueryRecord
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string FederationIdentifier { get; set; }
        public string Default_Franchise__c { get; set; }
        public string HI_GUID__c { get; set; }
        public bool IsActive { get; set; }
    }
}