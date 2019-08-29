using System.Threading.Tasks;
using Azure_AD_Users_Publisher.Services;
using Azure_AD_Users_Publisher.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Azure_AD_Users_Publisher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishController : ControllerBase
    {
        private readonly IHISCTokenService _tokenService;
        private readonly IProgramDataService _programDataService;

        public PublishController(IHISCTokenService tokenService, IProgramDataService programDataService)
        {
            _tokenService = tokenService;
            _programDataService = programDataService;
        }

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
    }
}