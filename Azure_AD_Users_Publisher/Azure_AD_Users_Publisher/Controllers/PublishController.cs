﻿using System.Collections.Generic;
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
                SalesforceFranchises = await salesforceFranchisesTask,
                ClearCareFranchises = await clearCareFranchisesTask
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

            //var userJson = "{\"FirstName\":\"Nikki\",\"LastName\":\"Sage\",\"Email\":\"nikki.sage@homeinstead.com\",\"FranchiseNumber\":\"3009\",\"OperatingSystem\":\"ClearCare\",\"ExternalId\":\"dc7287da-806a-4e8f-aea0-d2b1722c6b1a\",\"FederationId\":\"nikki.sage@homeinstead.com\",\"MobilePhone\":null,\"Address\":\"2009 Long Lake Rd, Suite 303\",\"City\":\"Sudbury\",\"State\":\"Ontario\",\"PostalCode\":\"P3E 6C3\",\"CountryCode\":null,\"TimeZone\":\"America/Toronto\",\"IsOwner\":false}";
            //var user = System.Text.Json.JsonSerializer.Deserialize<AzureActiveDirectoryUser>(userJson);

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