using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [HttpGet("currentSalesforceFranchises")]
        public async Task<IActionResult> Get()
        {
            var token = await _tokenService.RetrieveToken();
            var salesforceFranchisesTask = _programDataService.RetrieveFranchises(ProgramDataSources.Salesforce, token);
            var clearCareFranchisesTask = _programDataService.RetrieveFranchises(ProgramDataSources.ClearCare, token);
            await Task.WhenAll(salesforceFranchisesTask, clearCareFranchisesTask);

            var salesforceFranchises = await salesforceFranchisesTask;
            var clearCareFranchises = await clearCareFranchisesTask;

            var salesforce = $"[{string.Join(',', salesforceFranchises)}]";
            var clearCare = $"[{string.Join(',', clearCareFranchises)}]";
            var stringOutput = @"{""salesforceFranchises"":" + salesforce + @",""clearCareFranchises"":" + clearCare + "}";

            return Ok(stringOutput);
        }

        [ProducesResponseType(typeof(TimeZoneCountryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [HttpGet("timeZone")]
        public async Task<IActionResult> TimeZoneAndCountry([FromQuery] string address, [FromQuery] string city, [FromQuery] string state, [FromQuery] string postalCode)
        {
            var user = new AzureActiveDirectoryUser
            {
                Address = address, 
                City = city, 
                State = state, 
                PostalCode = postalCode
            };

            //var userJson = "{\"FirstName\":\"Nikki\",\"LastName\":\"Sage\",\"Email\":\"nikki.sage@homeinstead.com\",\"FranchiseNumber\":\"3009\",\"OperatingSystem\":\"ClearCare\",\"ExternalId\":\"dc7287da-806a-4e8f-aea0-d2b1722c6b1a\",\"FederationId\":\"nikki.sage@homeinstead.com\",\"MobilePhone\":null,\"Address\":\"2009 Long Lake Rd, Suite 303\",\"City\":\"Sudbury\",\"State\":\"Ontario\",\"PostalCode\":\"P3E 6C3\",\"CountryCode\":null,\"TimeZone\":\"America/Toronto\",\"IsOwner\":false}";
            //var user = System.Text.Json.JsonSerializer.Deserialize<AzureActiveDirectoryUser>(userJson);

            var timeZone = await _timeZoneService.RetrieveTimeZoneAndPopulateUsersCountryCode(user);
            var response = new TimeZoneCountryResponse
            {
                SalesforceSupporedTimeZone = timeZone,
                User = user
            };

            return Ok(response);
        }

        [ExcludeFromCodeCoverage]
        public class TimeZoneCountryResponse
        {
            public string SalesforceSupporedTimeZone { get; set; }
            public AzureActiveDirectoryUser User { get; set; }
        }
    }
}