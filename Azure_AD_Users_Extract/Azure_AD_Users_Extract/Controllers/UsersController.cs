using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure_AD_Users_Extract.Services;
using Azure_AD_Users_Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Azure_AD_Users_Extract.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IFranchiseUserService _franchiseUserService;

        public UsersController(IFranchiseUserService franchiseUserService)
        {
            _franchiseUserService = franchiseUserService;
        }

        // GET users
        [ProducesResponseType(typeof(List<AzureActiveDirectoryUser>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [HttpGet("franchise")]
        public async Task<IActionResult> Franchise([FromQuery]string groupId, [FromQuery]int syncDurationInHours = 0)
        {
            var users = await _franchiseUserService.GetFranchiseUsers(groupId, syncDurationInHours);
            return Ok(users);
        }

        [ProducesResponseType(typeof(List<AzureActiveDirectoryUser>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [HttpGet("filteredFranchise")]
        public async Task<IActionResult> FranchiseFilteredUsers([FromQuery]string groupId, string officeLocation, [FromQuery]int syncDurationInHours = 0)
        {
            var users = await _franchiseUserService.GetFranchiseUsers(groupId, syncDurationInHours);
            var filteredUsers = users.Where(user => user.FranchiseNumber.Equals(officeLocation)).ToList();
            return Ok(filteredUsers);
        }
    }
}