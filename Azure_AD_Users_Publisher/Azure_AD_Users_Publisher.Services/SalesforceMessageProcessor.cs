using System;
using System.Threading;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services.Interfaces;
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
            var bearerToken = await _tokenService.RetrieveToken();

            // note: seeing transient errors where when we use the token immediately the first usage of the token fails with a 401
            //       however the second call works and subsequent calls work.  adding a delay has solved the transient issue
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            var salesforceFranchisesTask = _programDataService.RetrieveFranchises(ProgramDataSources.Salesforce, bearerToken);
            var clearCareFranchisesTask = _programDataService.RetrieveFranchises(ProgramDataSources.ClearCare, bearerToken);
            await Task.WhenAll(salesforceFranchisesTask, clearCareFranchisesTask);

            var salesforceFranchises = await salesforceFranchisesTask;
            var clearCareFranchises = await clearCareFranchisesTask;
        }
    }
}