using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Interfaces;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Azure_AD_Users_Publisher.Services
{
    public class SubscriptionClientHostedService : IHostedService
    {
        private const string _nameToken = "SubscriptionClientHostedService";

        private readonly ILogger<SubscriptionClientHostedService> _logger;
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        private readonly IMessageProcessor _messageProcessor;
        private readonly string _topicName;
        private readonly string _subscriptionName;
        private readonly string _serviceBusConnectionStringSecretName;
        private SubscriptionClient _subscriptionClient;

        public SubscriptionClientHostedService(ILogger<SubscriptionClientHostedService> logger,
            IAzureKeyVaultService azureKeyVaultService,
            IConfiguration configuration,
            IMessageProcessor messageProcessor)
        {
            _logger = logger;
            _azureKeyVaultService = azureKeyVaultService;
            _messageProcessor = messageProcessor;
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

            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false
            };

            // Register the function that processes messages.
            _subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"{_nameToken} is stopping.");
            await _subscriptionClient.CloseAsync();
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            _logger.LogError("Exception context for troubleshooting:");
            _logger.LogError($"- Endpoint: {context.Endpoint}");
            _logger.LogError($"- Entity Path: {context.EntityPath}");
            _logger.LogError($"- Executing Action: {context.Action}");

            return Task.CompletedTask;
        }   

        private async Task ProcessMessagesAsync(Message message, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            // note: right now we only have one way to process messages. however, we are expecting to have more soon. therefore, 
            //       when we do we could register multiple IMessageProcessors and have the constructor take an enumeration
            //       then filter use the appropriate processor for the appropriate message
            await _messageProcessor.ProcessMessage(_subscriptionClient, message, cancellationToken);
        }
    }
}