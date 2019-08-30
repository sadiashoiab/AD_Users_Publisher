﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure_AD_Users_Shared.Exceptions;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _topicName;
        private readonly string _serviceBusConnectionStringSecretName;
        private readonly int _reoccurrenceInMinutes;

        private Timer _reoccurrenceTimer;
        private TopicClient _topicClient;
        private string _franchiseUsersReoccurrenceGroupId;
        private string _franchiseUsersReoccurrenceSyncDurationInHours;
        private string _azureADUsersFranchiseExtractUrl;

        public TopicClientHostedService(ILogger<TopicClientHostedService> logger,
            IAzureKeyVaultService azureKeyVaultService,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _azureKeyVaultService = azureKeyVaultService;
            _httpClientFactory = httpClientFactory;

            _topicName = configuration["ExtractTopicName"];
            _serviceBusConnectionStringSecretName = configuration["ServiceBusConnectionStringSecretName"];
            _reoccurrenceInMinutes = int.Parse(configuration["ReoccurrenceInMinutes"]);
            _azureADUsersFranchiseExtractUrl = configuration["AzureADUsersFranchiseExtractUrl"];
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
            _franchiseUsersReoccurrenceSyncDurationInHours = await franchiseUsersReoccurrenceSyncDurationInHoursTask;

            _topicClient = new TopicClient(serviceBusConnectionString, _topicName);

            cancellationToken.Register(async () =>
            {
                await _topicClient.CloseAsync();
                _logger.LogDebug($"{_nameToken} has called CloseAsync because of cancel.");
            });

            // note: create the time, but wait 10 seconds before we start executing work from it.
            _reoccurrenceTimer = new Timer(RetrieveAndProcessExtractUsers, null, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(_reoccurrenceInMinutes));
        }

        private async void RetrieveAndProcessExtractUsers(object state)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogDebug($"Starting retrieval and processing of franchise users at {DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}.");

            // todo: Sadia do we need to have both of these? can we just have one publisher application?
            var url = $"{_azureADUsersFranchiseExtractUrl}?groupId={_franchiseUsersReoccurrenceGroupId}&syncDurationInHours={_franchiseUsersReoccurrenceSyncDurationInHours}";
            var responseMessage = await SendAsync(url);
            var json = await responseMessage.Content.ReadAsStringAsync();
            var results = System.Text.Json.JsonSerializer.Deserialize<List<SalesforceUser>>(json);

            foreach (var salesforceUser in results)
            {
                var salesforceUserJson = System.Text.Json.JsonSerializer.Serialize(salesforceUser);
                var message = new Message(Encoding.UTF8.GetBytes(salesforceUserJson));
                await _topicClient.SendAsync(message);
            }

            var endTime = DateTime.UtcNow;
            _logger.LogDebug($"Finished retrieval and processing of franchise users at {DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}.");
            var userString =  (results.Count > 1) ? "user" : "users";
            _logger.LogDebug($"Sent {results.Count} {userString} to the topic in {(endTime - startTime).TotalSeconds} seconds.");
        }

        private async Task<HttpResponseMessage> SendAsync(string url)
        {
            var client = _httpClientFactory.CreateClient("ExtractHttpClient");

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            var responseMessage = await client.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new UnexpectedStatusCodeException(responseMessage);
            }

            if (responseMessage.Content == null)
            {
                throw new UnexpectedDataException(nameof(responseMessage.Content));
            }

            return responseMessage;
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
