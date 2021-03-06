﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace Azure_AD_Users_Shared.Models
{
    [ExcludeFromCodeCoverage]
    public class AzureActiveDirectoryUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string FranchiseNumber { get; set; }
        public string OperatingSystem { get; set; }
        public string ExternalId { get; set; }
        public string FederationId { get; set; }
        public string MobilePhone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Title { get; set; }
        public string CountryCode { get; set; }
        public string TimeZone { get; set; }
        public bool IsOwner => !string.IsNullOrWhiteSpace(Title) && Title.ToLower() == "franchise owner";
        public DateTimeOffset? DeactivationDateTimeOffset { get; set; }
    }
}