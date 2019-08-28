using System.Collections.Generic;
using System.Threading.Tasks;
using AD_Users_Extract.Controllers;
using AD_Users_Extract.Services;
using AD_Users_Extract.Services.Interfaces;
using AD_Users_Extract.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AD_Users_Extract.Tests
{
    [TestClass]
    public class UserControllerTests
    {
        [TestMethod]
        public async Task FranchiseUsersGivenValidParameters_ReturnsValidResults()
        {
            // ARRANGE
            var loggingMock = new Mock<ILogger<UsersController>>();
            var tokenServiceMock = new Mock<ITokenService>();
            var userServiceMock = new Mock<IUserService>();
            var unitUnderTest = new UsersController(loggingMock.Object, tokenServiceMock.Object, userServiceMock.Object);
            var groupId = "";
            var syncDuration = 24;
            var token = "ABC";

            tokenServiceMock.Setup(mock => mock.RetrieveToken(TokenEnum.Franchise))
                .ReturnsAsync(() => token).Verifiable();

            userServiceMock.Setup(mock => mock.GetUsers(groupId, token, syncDuration))
                .ReturnsAsync(() => new List<GraphUser> {new GraphUser()}).Verifiable();

            // ACT
            var results = await unitUnderTest.Franchise(groupId, syncDuration);

            // ASSERT
            tokenServiceMock.Verify();
            Assert.AreEqual(Microsoft.AspNetCore.Http.StatusCodes.Status200OK, (results as OkObjectResult)?.StatusCode);
        }
    }
}
