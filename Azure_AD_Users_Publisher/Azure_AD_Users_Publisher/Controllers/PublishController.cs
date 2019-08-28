using System.Collections.Generic;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services;
using Azure_AD_Users_Publisher.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Azure_AD_Users_Publisher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IProgramDataService _programDataService;

        public PublishController(ITokenService tokenService, IProgramDataService programDataService)
        {
            _tokenService = tokenService;
            _programDataService = programDataService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var token = await _tokenService.RetrieveToken();
            var salesforceFranchisesTask = _programDataService.RetrieveFranchises(ProgramDataSources.Salesforce, token);
            var clearCareFranchisesTask = _programDataService.RetrieveFranchises(ProgramDataSources.ClearCare, token);
            await Task.WhenAll(salesforceFranchisesTask, clearCareFranchisesTask);

            var salesforceFranchises = await salesforceFranchisesTask;
            var clearCareFranchises = await clearCareFranchisesTask;

            // peek-lock topic from bus
            // topic name:              franchiseusers
            // topic subscription name: salesforcefranchiseuserssubscription
            // maximum message count:   175
            // subscription type:       Main
            // connection:              RootManageSharedAccessKey


            // if we have one ore more results from the topic
            //var topics = new List<string>();
            //if (true)
            //{
            //    foreach (var topic in topics)
            //    {
                    
            //    }
            //}

            //_queueClient.RegisterMessageHandler(
            //    async (message, token) =>
            //    {
            //        try
            //        {
            //            processMessage(message); // await Task.Delay(...)
            //            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
            //        }
            //        catch (Exception ex)
            //        {
            //            await _queueClient.DeadLetterAsync(message.SystemProperties.LockToken);
            //        }
            //    },
            //    new MessageHandlerOptions(OnException)
            //    {
            //        AutoComplete = false,
            //        MaxConcurrentCalls = MaxConcurrentCalls,
            //        MaxAutoRenewDuration = MaxAutoRenewDuration
            //    }
            //);

            return Ok("value");
        }
    }
}

// https://prod-24.centralus.logic.azure.com:443/workflows/27dfb5ebbe844a1595d1a7ec0bd2575a/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=wVh5Ycww6R2qqY-ck_w2iWSG7Ij3ASTvS3FmeBpv-_w