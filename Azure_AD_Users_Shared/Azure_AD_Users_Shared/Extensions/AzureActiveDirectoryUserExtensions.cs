using System;
using System.Collections.Generic;
using System.Text;
using Azure_AD_Users_Shared.Models;

namespace Azure_AD_Users_Shared.Extensions
{
    public static class AzureActiveDirectoryUserExtensions
    {
        public static string ToCsv(this IEnumerable<AzureActiveDirectoryUser> users)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\"FirstName\",\"LastName\",\"Email\",\"FranchiseNumber\",\"OperatingSystem\",\"ExternalId\",\"FederationId\",\"MobilePhone\",\"Address\",\"City\",\"State\",\"PostalCode\",\"Title\",\"CountryCode\",\"TimeZone\",\"IsOwner\",\"DeactivationDateTimeOffset\"");
            foreach (var user in users)
            {
                sb.AppendLine($"\"{user.FirstName}\",\"{user.LastName}\",\"{user.Email}\",\"{user.FranchiseNumber}\",\"{user.OperatingSystem}\",\"{user.ExternalId}\",\"{user.FederationId}\",\"{user.MobilePhone}\",\"{user.Address}\",\"{user.City}\",\"{user.State}\",\"{user.PostalCode}\",\"{user.Title}\",\"{user.CountryCode}\",\"{user.TimeZone}\",\"{user.IsOwner}\",\"{user.DeactivationDateTimeOffset}\"");
            }

            return sb.ToString();
        }
    }
}
