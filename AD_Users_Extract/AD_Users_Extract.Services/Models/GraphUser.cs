using System;
using System.Diagnostics.CodeAnalysis;

namespace AD_Users_Extract.Services.Models
{
    [ExcludeFromCodeCoverage]
    public class GraphUser
    {
        //[JsonProperty("odata.type")]
        public string odatatype { get; set; }

        public string id { get; set; }

        //    public object deletedDateTime { get; set; }
        //    public bool accountEnabled { get; set; }
        //    public object ageGroup { get; set; }
        //    public List<object> businessPhones { get; set; }

        public string city { get; set; }

        //    public DateTime createdDateTime { get; set; }

        public string companyName { get; set; }

        //    public object consentProvidedForMinor { get; set; }

        public string country { get; set; }

        //    public object department { get; set; }
        //    public string displayName { get; set; }
        //    public object employeeId { get; set; }
        //    public object faxNumber { get; set; }

        public string givenName { get; set; }

        //    public List<string> imAddresses { get; set; }
        //    public object isResourceAccount { get; set; }
        
        public string jobTitle { get; set; }

        //    public object legalAgeGroupClassification { get; set; }

        public string mail { get; set; }

        //    public string mailNickname { get; set; }

        public string mobilePhone { get; set; }

        //    public object onPremisesDistinguishedName { get; set; }
        //    public object onPremisesDomainName { get; set; }
        //    public object onPremisesImmutableId { get; set; }

        public DateTimeOffset? onPremisesLastSyncDateTime { get; set; }

        //    public object onPremisesSecurityIdentifier { get; set; }
        //    public object onPremisesSamAccountName { get; set; }
        //    public object onPremisesSyncEnabled { get; set; }
        //    public object onPremisesUserPrincipalName { get; set; }
        //    public List<object> otherMails { get; set; }
        //    public string passwordPolicies { get; set; }
        //    public object passwordProfile { get; set; }

        public string postalCode { get; set; }

        //    public object preferredDataLocation { get; set; }
        //    public string preferredLanguage { get; set; }
        //    public List<string> proxyAddresses { get; set; }
        //    public DateTime refreshTokensValidFromDateTime { get; set; }
        //    public object showInAddressList { get; set; }

        public string state { get; set; }
        public string streetAddress { get; set; }

        public string surname { get; set; }

        //    public string usageLocation { get; set; }

        public string userPrincipalName { get; set; }

        public string officeLocation { get; set; }

        //    public object externalUserState { get; set; }
        //    public object externalUserStateChangeDateTime { get; set; }
        //    public string userType { get; set; }
        //    public List<AssignedLicens> assignedLicenses { get; set; }
        //    public List<AssignedPlan> assignedPlans { get; set; }
        //    public List<object> deviceKeys { get; set; }

        public OnPremisesExtensionAttributes onPremisesExtensionAttributes { get; set; }

        //    public List<object> onPremisesProvisioningErrors { get; set; }
        //    public List<ProvisionedPlan> provisionedPlans { get; set; }

        public string timeZoneId { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class OnPremisesExtensionAttributes
    {
        //public object extensionAttribute1 { get; set; }
        //public object extensionAttribute2 { get; set; }
        //public object extensionAttribute3 { get; set; }

        public string extensionAttribute4 { get; set; }
        //public object extensionAttribute5 { get; set; }
        //public object extensionAttribute6 { get; set; }
        //public object extensionAttribute7 { get; set; }

        public DateTimeOffset? extensionAttribute8 { get; set; }
        
        //public object extensionAttribute9 { get; set; }
        //public object extensionAttribute10 { get; set; }
        //public object extensionAttribute11 { get; set; }
        //public object extensionAttribute12 { get; set; }
        //public object extensionAttribute13 { get; set; }
        //public object extensionAttribute14 { get; set; }
        //public object extensionAttribute15 { get; set; }
    }
}
