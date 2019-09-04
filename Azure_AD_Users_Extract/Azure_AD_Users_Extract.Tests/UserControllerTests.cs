using System.Collections.Generic;
using System.Threading.Tasks;
using Azure_AD_Users_Extract.Controllers;
using Azure_AD_Users_Extract.Services;
using Azure_AD_Users_Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Azure_AD_Users_Extract.Tests
{
    [TestClass]
    public class UserControllerTests
    {
        [TestMethod]
        public async Task FranchiseUsersGivenValidParameters_ReturnsValidResults()
        {
            // ARRANGE
            var franchiseUserServiceMock = new Mock<IFranchiseUserService>();
            var unitUnderTest = new UsersController(franchiseUserServiceMock.Object);
            var groupId = "";
            var syncDuration = 24;

            franchiseUserServiceMock.Setup(mock => mock.GetFranchiseUsers(groupId, syncDuration))
                .ReturnsAsync(() => new List<AzureActiveDirectoryUser> {new AzureActiveDirectoryUser()}).Verifiable();

            // ACT
            var results = await unitUnderTest.FranchiseFilteredUsers(groupId, "838", syncDuration);

            // ASSERT
            franchiseUserServiceMock.Verify();
            Assert.AreEqual(Microsoft.AspNetCore.Http.StatusCodes.Status200OK, (results as OkObjectResult)?.StatusCode);
        }
    }
}
