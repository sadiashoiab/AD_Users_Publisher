using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Azure_AD_Users_Publisher.Services
{
    public class SalesforceMessageProcessor : IMessageProcessor
    {
        private readonly ILogger<SalesforceMessageProcessor> _logger;

        public SalesforceMessageProcessor(ILogger<SalesforceMessageProcessor> logger)
        {
            _logger = logger;
        }

        public async Task ProcessMessage(ISubscriptionClient receiver,
            Message message,
            CancellationToken cancellationToken)
        {
            if (message.Label != null &&
                message.ContentType != null &&
                //message.Label.Equals("Scientist", StringComparison.InvariantCultureIgnoreCase) &&
                message.ContentType.Equals("application/json", StringComparison.InvariantCultureIgnoreCase))
            {
                // note: Use the cancellationToken passed as necessary to determine if the subscriptionClient has already been closed.
                // If subscriptionClient has already been closed, choosing not to process the message to avoid unnecessary exceptions.
                if (!cancellationToken.IsCancellationRequested)
                {
                    // Process the message.
                    var body = message.Body;

                    //dynamic scientist = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(body));

                    //lock (Console.Out)
                    //{
                    //    Console.ForegroundColor = color;
                    //    Console.WriteLine(
                    //        "\t\t\t\tMessage received: \n\t\t\t\t\t\tMessageId = {0}, \n\t\t\t\t\t\tSequenceNumber = {1}, \n\t\t\t\t\t\tEnqueuedTimeUtc = {2}," +
                    //        "\n\t\t\t\t\t\tExpiresAtUtc = {5}, \n\t\t\t\t\t\tContentType = \"{3}\", \n\t\t\t\t\t\tSize = {4},  \n\t\t\t\t\t\tContent: [ firstName = {6}, name = {7} ]",
                    //        message.MessageId,
                    //        message.SystemProperties.SequenceNumber,
                    //        message.SystemProperties.EnqueuedTimeUtc,
                    //        message.ContentType,
                    //        message.Size,
                    //        message.ExpiresAtUtc,
                    //        scientist.firstName,
                    //        scientist.name);
                    //    Console.ResetColor();
                    //}

                    // Complete the message so that it is not received again.
                    // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
                    await receiver.CompleteAsync(message.SystemProperties.LockToken);
                }
            }
            else
            {
                _logger.LogError($"Processing Error, Malformed message and do not know how to process. Moving to Dead Letter");
                await receiver.DeadLetterAsync(message.SystemProperties.LockToken, "ProcessingError",
                    "Don't know what to do with this message");
            }
        }
    }
}