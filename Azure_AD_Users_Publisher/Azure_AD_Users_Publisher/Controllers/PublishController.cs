using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services;
using Azure_AD_Users_Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Azure_AD_Users_Publisher.Controllers
{
    // todo: remove the ExcludeFromCodeCoverage once we know what endpoints and functionality we desire
    //       right now this is just a placeholder controller and serves no purpose for the app
    [ExcludeFromCodeCoverage]
    [Route("api/[controller]")]
    [ApiController]
    public class PublishController : ControllerBase
    {
        private readonly IHISCTokenService _tokenService;
        private readonly IProgramDataService _programDataService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMessageProcessor _messageProcessor;

        public PublishController(IHISCTokenService tokenService, IProgramDataService programDataService, ITimeZoneService timeZoneService, IMessageProcessor messageProcessor)
        {
            _tokenService = tokenService;
            _programDataService = programDataService;
            _timeZoneService = timeZoneService;
            _messageProcessor = messageProcessor;
        }

        [ProducesResponseType(typeof(FranchiseResults), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [HttpGet("currentProgramDataFranchises")]
        public async Task<IActionResult> Get()
        {
            var token = await _tokenService.RetrieveToken();
            var salesforceFranchisesTask = _programDataService.RetrieveFranchises(ProgramDataSources.Salesforce, token, false);
            var clearCareFranchisesTask = _programDataService.RetrieveFranchises(ProgramDataSources.ClearCare, token, false);
            await Task.WhenAll(salesforceFranchisesTask, clearCareFranchisesTask);

            var franchiseResults = new FranchiseResults
            {
                SalesforceFranchises = (await salesforceFranchisesTask).OrderBy(i => i).ToArray(),
                ClearCareFranchises = (await clearCareFranchisesTask).OrderBy(i => i).ToArray()
            };

            return Ok(franchiseResults);
        }

        [ProducesResponseType(typeof(AzureActiveDirectoryUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [HttpGet("populateUserCountryAndSalesforceSupportedTimeZone")]
        public async Task<IActionResult> TimeZoneAndCountry([FromQuery] string franchiseNumber, [FromQuery] string address, [FromQuery] string city, [FromQuery] string state, [FromQuery] string postalCode)
        {
            var user = new AzureActiveDirectoryUser
            {
                FranchiseNumber = franchiseNumber,
                Address = address, 
                City = city, 
                State = state, 
                PostalCode = postalCode
            };

            var timeZone = await _timeZoneService.RetrieveTimeZoneAndPopulateUsersCountryCode(user);
            user.TimeZone = timeZone;
            
            return Ok(user);
        }

        // note: quick way to test a given user, not needed for production, still determining if we need this for testing
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[Produces("application/json")]
        //[HttpGet("salesforceUser")]
        //public async Task<IActionResult> IsSalesforceUser()
        //{
        //    var user = new AzureActiveDirectoryUser
        //    {
        //        // db12fc65-c9e1-4c32-a9b8-97b14229d166 -- IsActive: false
        //        // e62d778a-db0a-45d4-8829-e638065ae248 -- IsActive: true
        //        ExternalId = "db12fc65-c9e1-4c32-a9b8-97b14229d166",
        //        DeactivationDateTimeOffset = DateTimeOffset.Now.AddDays(-1)
        //    };

        //    await _messageProcessor.ProcessUser(user);
            
        //    return Ok();
        //}

        [ExcludeFromCodeCoverage]
        public class FranchiseResults
        {
            public int[] SalesforceFranchises { get; set; }
            public int[] ClearCareFranchises { get; set; }
        }
    }
}