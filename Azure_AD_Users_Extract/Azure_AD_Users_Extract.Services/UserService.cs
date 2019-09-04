﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure_AD_Users_Extract.Services.Models;
using Azure_AD_Users_Shared.Models;
using Microsoft.Extensions.Configuration;

namespace Azure_AD_Users_Extract.Services
{
    public class UserService : IUserService
    {
        private const string _odataInitialName = "@odata.type";
        private const string _odataReplacementName = "odatatype";
        private const string _odataTypeGroupName = "#microsoft.graph.group";
        private const string _memberPropertiesToSelect = "$select=onPremisesLastSyncDateTime,companyName,givenName,mail,mobilePhone,surname,jobTitle,id,userPrincipalName,officeLocation,city,country,postalCode,state,streetAddress,onPremisesExtensionAttributes,businessPhones";

        private readonly IGraphApiService _graphApiService;
        private readonly string _graphApiUrl;

        public UserService(IConfiguration configuration, IGraphApiService graphApiService)
        {
            _graphApiUrl = configuration["GraphApiUrl"];
            _graphApiService = graphApiService;
        }

        public async Task<List<AzureActiveDirectoryUser>> GetUsers(string groupId, string token, int syncDurationInHours = 0)
        {
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(token) || syncDurationInHours < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var url = $"{_graphApiUrl}/groups/{groupId}/members?{_memberPropertiesToSelect}";
            var duration = syncDurationInHours * -1;
            var usersList = await GetGraphUsers(url, duration, token);
            await GetGraphGroupUsers(usersList, duration, token);
            var azureActiveDirectoryUsers = MapGraphUsersToAzureActiveDirectoryUsers(usersList);
            return azureActiveDirectoryUsers;
        }

        private List<AzureActiveDirectoryUser> MapGraphUsersToAzureActiveDirectoryUsers(List<GraphUser> usersList)
        {
            return usersList.Select(graphUser => new AzureActiveDirectoryUser
                {
                    FirstName = graphUser.givenName,
                    LastName = graphUser.surname,
                    Email = graphUser.mail,
                    FranchiseNumber = graphUser.officeLocation,
                    ExternalId = graphUser.id,
                    FederationId = graphUser.userPrincipalName,
                    MobilePhone = graphUser.mobilePhone,
                    Address = graphUser.streetAddress,
                    City = graphUser.city,
                    State = graphUser.state,
                    PostalCode = graphUser.postalCode,
                    CountryCode = graphUser.country,
                    IsOwner = graphUser.owner,
                    DeactivationDateTimeOffset = graphUser.onPremisesExtensionAttributes?.extensionAttribute8
                })
                .ToList();
        }

        private async Task<List<GraphUser>> GetGraphUsers(string url, int duration, string token)
        {
            bool keepIterating;
            var usersList = new List<GraphUser>();
            do
            {
                var json = await RetrieveAndAddGraphUsers(usersList, url, duration, token);
                keepIterating = AreMoreGraphUsersAvailable(json, out url);
            } while (keepIterating);

            return usersList;
        }

        private static bool AreMoreGraphUsersAvailable(string json, out string url)
        {
            url = null;
            var keepIterating = false;

            using (var jsonDocument = JsonDocument.Parse(json))
            {
                // nextLink -- This indicates there are additional pages of data to be retrieved in the session.
                var propertyExisted = jsonDocument.RootElement.TryGetProperty("@odata.nextLink", out var nextLinkJsonElement);
                if (propertyExisted)
                {
                    var nextUrl = nextLinkJsonElement.GetString();
                    if (!string.IsNullOrWhiteSpace(nextUrl))
                    {
                        url = nextUrl;
                        keepIterating = true;
                    }
                }
            }

            return keepIterating;
        }

        // note: i do not have a "live" example of this occuring, however a mocked a unit test that does
        private async Task GetGraphGroupUsers(List<GraphUser> usersList, int duration, string token)
        {
            var groupUsers = usersList.FindAll(user => user.odatatype.Equals(_odataTypeGroupName));
            do
            {
                foreach (var groupUser in groupUsers)
                {
                    if (groupUser.odatatype.Equals(_odataTypeGroupName))
                    {
                        var url = $"{_graphApiUrl}/groups/{groupUser.id}/members?$top=999&{_memberPropertiesToSelect}";
                        var _ = await RetrieveAndAddGraphUsers(usersList, url, duration, token);
                    }

                    usersList.RemoveAll(user => user.id == groupUser.id);
                }

                groupUsers = usersList.FindAll(user => user.odatatype.Equals(_odataTypeGroupName));
            } while (groupUsers.Count > 0);
        }

        private async Task<string> RetrieveAndAddGraphUsers(List<GraphUser> usersList, string url, int duration, string token)
        {
            var json = await _graphApiService.RetrieveData(url, token);
            var cleanedJson = json.Replace(_odataInitialName, _odataReplacementName);
            var rootObject = JsonSerializer.Deserialize<GraphRootObject>(cleanedJson);
            var allUsers = rootObject.value.ToList();
            var filteredUsers = ApplyUserFilters(allUsers, duration);
            usersList.AddRange(filteredUsers);
            return json;
        }

        private List<GraphUser> ApplyUserFilters(List<GraphUser> usersList, int duration)
        {
            // note: if duration is 0, we want all the users and do NOT want to filter them
            if (duration == 0)
            {
                return usersList;
            }

            var usersToKeep = usersList.Where(user =>
                user.onPremisesLastSyncDateTime.HasValue &&
                user.onPremisesLastSyncDateTime.Value > DateTimeOffset.UtcNow.AddHours(duration)).ToList();

            return usersToKeep;
        }
    }
}