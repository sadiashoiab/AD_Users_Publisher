using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestBed.Services;
using TestBed.Services.Interfaces;

namespace TestBed.Tests
{
    [TestClass]
    public class TokenServiceTests
    {
        //private static TestContext _context;

        //[ClassInitialize]
        //public static void Initialize(TestContext context)
        //{
        //    _context = context;
        //    _context.Properties["config"] = new ConfigurationBuilder()
        //        .AddJsonFile("token-service-test-settings.json")
        //        .Build();
        //}

        [TestMethod]
        public async Task RetrieveFranchiseToken()
        {
            // ARRANGE
            //var config = (IConfiguration) _context.Properties["config"];
            var keyVaultServiceMock = new Mock<IAzureKeyVaultService>();
            var tokenProvider = new Mock<IGraphApiService>();
            var unitUnderTest = new TokenService(keyVaultServiceMock.Object, tokenProvider.Object);

            var clientId = "CLIENTID";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("NAClientId"))
                .ReturnsAsync(() => clientId).Verifiable();
            
            var appKey = "APPKEY";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("NAAppKey"))
                .ReturnsAsync(() => appKey).Verifiable();

            var tenant = "TENANT";
            keyVaultServiceMock.Setup(mock => mock.GetSecret("NATenant"))
                .ReturnsAsync(() => tenant).Verifiable();

            var expectedToken = "TOKEN";
            tokenProvider.Setup(mock => mock.AcquireToken(clientId, appKey, tenant))
                .ReturnsAsync(() => expectedToken).Verifiable();

            // ACT
            var result = await unitUnderTest.RetrieveToken(TokenEnum.Franchise);

            // ASSERT
            keyVaultServiceMock.Verify();
            Assert.AreEqual(expectedToken, result);
            
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task RetrieveTokenWithNonImplementedToken_ArgumentOutOfRangeException()
        {
            // ARRANGE
            var keyVaultServiceMock = new Mock<IAzureKeyVaultService>();
            var tokenProvider = new Mock<IGraphApiService>();
            var unitUnderTest = new TokenService(keyVaultServiceMock.Object, tokenProvider.Object);

            // ACT
            var _ = await unitUnderTest.RetrieveToken(TokenEnum.HomeOffice);
        }
    }
}
