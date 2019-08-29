using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Interfaces;
using Azure_AD_Users_Publisher.Services.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Azure_AD_Users_Publisher.Services
{
    public class SalesforceMessageProcessor : IMessageProcessor
    {
        private readonly ILogger<SalesforceMessageProcessor> _logger;
        private readonly ITokenService _tokenService;
        private readonly IProgramDataService _programDataService;

        public SalesforceMessageProcessor(ILogger<SalesforceMessageProcessor> logger, ITokenService tokenService, IProgramDataService programDataService)
        {
            _logger = logger;
            _tokenService = tokenService;
            _programDataService = programDataService;
        }

        public async Task ProcessMessage(ISubscriptionClient receiver,
            Message message,
            CancellationToken cancellationToken)
        {
            var token = await _tokenService.RetrieveToken();

            // note: i seeing transient errors where when we use the token immediately the first usage of the token fails with a 401
            //       however the second call works and subsequent calls work.  adding a delay has solved the transient issue
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            var salesforceFranchisesTask = _programDataService.RetrieveFranchises(ProgramDataSources.Salesforce, token);
            var clearCareFranchisesTask = _programDataService.RetrieveFranchises(ProgramDataSources.ClearCare, token);
            await Task.WhenAll(salesforceFranchisesTask, clearCareFranchisesTask);

            var salesforceFranchises = await salesforceFranchisesTask;
            var clearCareFranchises = await clearCareFranchisesTask;

            //if (message.Label != null &&
            //    message.ContentType != null &&
            //    //message.Label.Equals("Scientist", StringComparison.InvariantCultureIgnoreCase) &&
            //    message.ContentType.Equals("application/json", StringComparison.InvariantCultureIgnoreCase))
            //{

                // note: Use the cancellationToken passed as necessary to determine if the subscriptionClient has already been closed.
                // If subscriptionClient has already been closed, choosing not to process the message to avoid unnecessary exceptions.
                //if (!cancellationToken.IsCancellationRequested)
                //{
                //    // Process the message.
                //    var messageBody = Encoding.UTF8.GetString(message.Body);
                //    var user = System.Text.Json.JsonSerializer.Deserialize<AzureADUserExtractMessageBody>(messageBody);




                //    // Complete the message so that it is not received again.
                //    // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
                //    await receiver.CompleteAsync(message.SystemProperties.LockToken);
                //}
            //}
            //else
            //{
            //    // todo: do we want this or just keep them out there?
            //    //_logger.LogError($"Processing Error, Malformed message and do not know how to process. Moving to Dead Letter");
            //    //await receiver.DeadLetterAsync(message.SystemProperties.LockToken, "ProcessingError",
            //    //    "Don't know what to do with this message");
            //}
        }
    }
}