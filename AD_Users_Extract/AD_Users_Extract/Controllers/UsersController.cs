﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AD_Users_Extract.Services;
using AD_Users_Extract.Services.Interfaces;
using AD_Users_Extract.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AD_Users_Extract.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;

        public UsersController(ILogger<UsersController> logger, ITokenService tokenService, IUserService userService)
        {
            _logger = logger;
            _tokenService = tokenService;
            _userService = userService;
        }

        // GET users
        [ProducesResponseType(typeof(List<GraphUser>), StatusCodes.Status200OK)]
        [HttpGet("franchise")]
        public async Task<IActionResult> Franchise([FromQuery]string groupId, [FromQuery]int syncDurationInHours = 0)
        {
            var token = await _tokenService.RetrieveToken(TokenEnum.Franchise);
            var users = await _userService.GetUsers(groupId, token, syncDurationInHours);

            if (users?.Count > 0)
            {
                _logger.LogInformation($"{users.Count} Franchise users were retrieved");
            }

            return Ok(users);
        }

        // note: this is here for a quick way to force an error
        //[ProducesResponseType(typeof(Exception), StatusCodes.Status500InternalServerError)]
        //[HttpGet("/error")]
        //public IActionResult ThrowException()
        //{
        //    var innerException = new Exception("this is the inner exception message");
        //    throw new Exception("this is the outer exception message", innerException);
        //}
    }
}