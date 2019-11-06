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
            _logger.LogDebug($"FranchiseUsersReoccurrenceSyncDurationInHours set to: {_franchiseUsersReoccurrenceSyncDurationInHours}.");

            try
            {
                _topicClient = new TopicClient(serviceBusConnectionString, _topicName);
                _logger.LogDebug($"TopicClient will be sending to Topic: {_nameToken}.");
                _logger.LogDebug(
                    $"ServiceBusConnectionStringSecretName is set to: {_serviceBusConnectionStringSecretName}");

                cancellationToken.Register(async () =>
                {
                    await _topicClient.CloseAsync();
                    _logger.LogDebug($"{_nameToken} has called CloseAsync because of cancel.");
                });

                if (_reoccurrenceInMinutes > 0)
                {
                    _logger.LogInformation($"ReoccurrenceInMinutes is set to: {_reoccurrenceInMinutes}, Creating timer to Retrieve and Process Extract Users");
                    // note: create the timer, but wait 10 seconds before we start executing work from it.
                    _reoccurrenceTimer = new Timer(RetrieveAndProcessExtractUsers,
                        null,
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromMinutes(_reoccurrenceInMinutes));
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An exception occurred when trying to initialize the TopicClient");
            }
        }

        private async void RetrieveAndProcessExtractUsers(object state)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation($"Starting retrieval and processing of franchise users at {DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}.");

            try
            {
                // note: doing these at the "same time" to better utilize resources and time
                var franchiseUsersTask = _franchiseUserService.GetFranchiseUsers(_franchiseUsersReoccurrenceGroupId, _franchiseUsersReoccurrenceSyncDurationInHours);
                var deactivatedUsersTask = _franchiseUserService.GetFranchiseDeactivatedUsers(_franchiseUsersReoccurrenceSyncDurationInHours);
                await Task.WhenAll(franchiseUsersTask, deactivatedUsersTask);

                // unwrap the task results to get the respective users
                var franchiseUsers = await franchiseUsersTask;
                var deactivatedUsers = await deactivatedUsersTask;

                // note: we do not want to update franchise users that have a deactivation date.  we have a separate api call where we are retrieving the
                //       deactivated users, therefore only send franchise users if they do not have a deactivation date set
                var franchiseUsersThatDoNotHaveADeactivationDateSet = franchiseUsers.Where(user => !user.DeactivationDateTimeOffset.HasValue).ToList();
                
                // note: doing these at the "same time" to better utilize resources and time
                var publishFranchiseUsersTask = SendUsersToServiceBusTopic(franchiseUsersThatDoNotHaveADeactivationDateSet);
                var publishDeactivatedUsers = SendUsersToServiceBusTopic(deactivatedUsers);
                await Task.WhenAll(publishFranchiseUsersTask, publishDeactivatedUsers);

                var endTime = DateTime.UtcNow;
                var elapsed = endTime - startTime;
                _logger.LogInformation(
                    $"Finished retrieval and processing of users at {endTime.ToString(CultureInfo.InvariantCulture)}.");

                var counts = franchiseUsersThatDoNotHaveADeactivationDateSet.Count + deactivatedUsers.Count;
                var userString = counts > 1 ? "user" : "users";
                _logger.LogInformation(
                    $"Sent {counts} {userString} to the topic: {_topicName} in {elapsed.TotalSeconds} seconds.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in Retrieve and Process with FranchiseUsersReoccurrenceGroupId: {_franchiseUsersReoccurrenceGroupId}, and FranchiseUsersReoccurrenceSyncDurationInHours: {_franchiseUsersReoccurrenceSyncDurationInHours}, StackTrace: {ex.StackTrace}, InnerException: {ex.InnerException}");
            }
        }

        private async Task SendUsersToServiceBusTopic(IEnumerable<AzureActiveDirectoryUser> users)
        {
            // note: think about converting this to use the SendAsync(IList<Message> messageList)
            foreach (var user in users)
            {
                var userJson = System.Text.Json.JsonSerializer.Serialize(user);
                var message = new Message(Encoding.UTF8.GetBytes(userJson));
                await _topicClient.SendAsync(message);
            }
        }

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
