using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure_AD_Users_Shared.Models;
using Azure_AD_Users_Shared.Services;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Azure_AD_Users_Extract.Services
{
    public class TopicClientHostedService : IHostedService, IDisposable
    {
        private const string _nameToken = "TopicClientHostedService";

        private readonly ILogger<TopicClientHostedService> _logger;
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        private readonly IFranchiseUserService _franchiseUserService;
        private readonly string _topicName;
        private readonly string _serviceBusConnectionStringSecretName;
        private readonly int _reoccurrenceInMinutes;

        private Timer _reoccurrenceTimer;
        private TopicClient _topicClient;
        private string _franchiseUsersReoccurrenceGroupId;
        private int _franchiseUsersReoccurrenceSyncDurationInHours;

        public TopicClientHostedService(ILogger<TopicClientHostedService> logger,
            IAzureKeyVaultService azureKeyVaultService,
            IConfiguration configuration,
            IFranchiseUserService franchiseUserService)
        {
            _logger = logger;
            _azureKeyVaultService = azureKeyVaultService;
            _franchiseUserService = franchiseUserService;

            _topicName = configuration["ExtractTopicName"];
            _serviceBusConnectionStringSecretName = configuration["ServiceBusConnectionStringSecretName"];
            _reoccurrenceInMinutes = int.Parse(configuration["ReoccurrenceInMinutes"]);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"{_nameToken} is starting in the background.");
            var serviceBusConnectionStringTask = _azureKeyVaultService.GetSecret(_serviceBusConnectionStringSecretName);
            var franchiseUsersReoccurrenceGroupIdTask = _azureKeyVaultService.GetSecret("FranchiseUsersReoccurrenceGroupId");
            var franchiseUsersReoccurrenceSyncDurationInHoursTask = _azureKeyVaultService.GetSecret("FranchiseUsersReoccurrenceSyncDurationInHours");

            await Task.WhenAll(serviceBusConnectionStringTask, 
                franchiseUsersReoccurrenceGroupIdTask,
                franchiseUsersReoccurrenceSyncDurationInHoursTask);

            var serviceBusConnectionString = await serviceBusConnectionStringTask;
            _franchiseUsersReoccurrenceGroupId = await franchiseUsersReoccurrenceGroupIdTask;
            _franchiseUsersReoccurrenceSyncDurationInHours = int.Parse(await franchiseUsersReoccurrenceSyncDurationInHoursTask);

            _topicClient = new TopicClient(serviceBusConnectionString, _topicName);
            _logger.LogDebug($"TopicClient will be sending to Topic: {_nameToken}.");
            _logger.LogDebug($"ServiceBusConnectionStringSecretName is set to: {_serviceBusConnectionStringSecretName}.");

            cancellationToken.Register(async () =>
            {
                await _topicClient.CloseAsync();
                _logger.LogDebug($"{_nameToken} has called CloseAsync because of cancel.");
            });

            if (_reoccurrenceInMinutes > 0)
            {
                // note: create the timer, but wait 10 seconds before we start executing work from it.
                _reoccurrenceTimer = new Timer(RetrieveAndProcessExtractUsers,
                    null, 
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromMinutes(_reoccurrenceInMinutes));
            }
        }

        private async void RetrieveAndProcessExtractUsers(object state)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogDebug($"Starting retrieval and processing of franchise users at {DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}.");

            var results = await _franchiseUserService.GetFranchiseUsers(_franchiseUsersReoccurrenceGroupId, _franchiseUsersReoccurrenceSyncDurationInHours);

            // note: code to grab the salesforce deactivated users for research
            //var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);
            //var deactiveatedUsers = results.Where(user => user.DeactivationDateTimeOffset.HasValue && user.DeactivationDateTimeOffset.Value >= thirtyDaysAgo).ToList();

            //var franchisesToMonitor = new int[]
            //{
            //    100,
            //    101,
            //    149,
            //    169,
            //    193,
            //    197,
            //    203,
            //    211,
            //    234,
            //    235,
            //    238,
            //    244,
            //    295,
            //    308,
            //    334,
            //    363,
            //    391,
            //    407,
            //    445,
            //    455,
            //    630,
            //    838,
            //    3009,
            //    3026
            //};

            //var salesforceDeactivatedUsers = new List<AzureActiveDirectoryUser>();
            //foreach (var franchise in franchisesToMonitor)
            //{
            //    var franchiseDeactivated = deactiveatedUsers
            //        .Where(user => user.FranchiseNumber.Equals(franchise.ToString())).ToList();
            //    salesforceDeactivatedUsers.AddRange(franchiseDeactivated);
            //}

            //var franchiseCount = salesforceDeactivatedUsers.Count;
            //var franchiseJson = System.Text.Json.JsonSerializer.Serialize(salesforceDeactivatedUsers);

            // todo: remove after development testing completes
            //var filteredExtractUsers = FilterUsers(results);

            foreach (var user in results)
            {
                var userJson = System.Text.Json.JsonSerializer.Serialize(user);
                var message = new Message(Encoding.UTF8.GetBytes(userJson));
                await _topicClient.SendAsync(message);
            }

            var endTime = DateTime.UtcNow;
            var elapsed = endTime - startTime;
            _logger.LogDebug($"Finished retrieval and processing of franchise users at {endTime.ToString(CultureInfo.InvariantCulture)}.");
            var userString =  results.Count > 1 ? "user" : "users";
            _logger.LogDebug($"Sent {results.Count} {userString} to the topic: {_topicName} in {elapsed.TotalSeconds} seconds.");
        }

        // todo: remove after development testing completes
        //private List<AzureActiveDirectoryUser> FilterUsers(List<AzureActiveDirectoryUser> results)
        //{
        //    var franchiseUsers = results.Where(user => user.FranchiseNumber != null && user.FranchiseNumber.Contains("3009")).ToList();
        //    var franchiseUsersJson = System.Text.Json.JsonSerializer.Serialize(franchiseUsers);
        //    _logger.LogDebug($"Filtered {results.Count} Users down to {franchiseUsers.Count} FranchiseNumber 407 Users{Environment.NewLine}{Environment.NewLine}{franchiseUsersJson}");
        //    return franchiseUsers;
        //}

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"{_nameToken} is stopping.");
            await _topicClient.CloseAsync();

            _reoccurrenceTimer?.Change(Timeout.Infinite, 0);
        }

        public void Dispose()
        {
            _reoccurrenceTimer?.Dispose();
        }
    }
}
