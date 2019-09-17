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

        public PublishController(IHISCTokenService tokenService, IProgramDataService programDataService, ITimeZoneService timeZoneService)
        {
            _tokenService = tokenService;
            _programDataService = programDataService;
            _timeZoneService = timeZoneService;
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

        [ExcludeFromCodeCoverage]
        public class FranchiseResults
        {
            public int[] SalesforceFranchises { get; set; }
            public int[] ClearCareFranchises { get; set; }
        }
    }
}