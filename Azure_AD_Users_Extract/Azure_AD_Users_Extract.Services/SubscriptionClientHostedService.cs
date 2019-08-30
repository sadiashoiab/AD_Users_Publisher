using System.Threading;
using System.Threading.Tasks;
using Azure_AD_Users_Shared.Services;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Azure_AD_Users_Extract.Services
{
    public class SubscriptionClientHostedService : IHostedService
    {
        private const string _nameToken = "SubscriptionClientHostedService";

        private readonly ILogger<SubscriptionClientHostedService> _logger;
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        private readonly string _topicName;
        private readonly string _subscriptionName;
        private readonly string _serviceBusConnectionStringSecretName;

        private SubscriptionClient _subscriptionClient;

        public SubscriptionClientHostedService(ILogger<SubscriptionClientHostedService> logger,
            IAzureKeyVaultService azureKeyVaultService,
            IConfiguration configuration)
        {
            _logger = logger;
            _azureKeyVaultService = azureKeyVaultService;
            _topicName = configuration["ExtractTopicName"];
            _subscriptionName = configuration["ExtractSubscriptionName"];
            _serviceBusConnectionStringSecretName = configuration["ServiceBusConnectionStringSecretName"];
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"{_nameToken} is starting in the background.");
            var serviceBusConnectionString = await _azureKeyVaultService.GetSecret(_serviceBusConnectionStringSecretName);
            _subscriptionClient = new SubscriptionClient(serviceBusConnectionString, _topicName, _subscriptionName);

            cancellationToken.Register(async () =>
            {
                await _subscriptionClient.CloseAsync();
                _logger.LogDebug($"{_nameToken} has called CloseAsync because of cancel.");
            });

            // todo: add timer
            //       load ReoccurrenceInMinutes setting for how often timer executes
            //       when timer executes, call endpoint and pull users to publish
            //       foreach user, publish to bus
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"{_nameToken} is stopping.");
            await _subscriptionClient.CloseAsync();

            // todo: stop timer and cleanup
        }
    }
}
