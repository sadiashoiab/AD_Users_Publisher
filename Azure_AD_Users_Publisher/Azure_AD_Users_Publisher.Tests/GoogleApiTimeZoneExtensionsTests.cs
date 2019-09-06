using Azure_AD_Users_Publisher.Services;
using Azure_AD_Users_Publisher.Services.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Azure_AD_Users_Publisher.Tests
{
    [TestClass]
    public class GoogleApiTimeZoneExtensionsTests
    {
        [TestMethod]
        public void OneMatches()
        {
            // ARRANGE
            // new SalesforceSupportedTimeZone(-36000, "Hawaii-Aleutian Standard Time", "Pacific/Honolulu"),
            var googleTimeZone = new GoogleApiTimeZone
            {
                rawOffset = -36000,
                dstOffset = 0
            };

            // ACT
            var result = googleTimeZone.ToSalesforceTimeZone();

            // ASSERT
            Assert.AreEqual("Pacific/Honolulu", result);
        }

        [TestMethod]
        public void MultipleTimeZoneNameMatches()
        {
            // ARRANGE
            var googleTimeZone = new GoogleApiTimeZone
            {
                rawOffset = -18000,
                dstOffset = 3600,
                timeZoneName = "Eastern Daylight Time"
            };

            // ACT
            var result = googleTimeZone.ToSalesforceTimeZone();

            // ASSERT
            Assert.AreEqual("America/Indiana/Indianapolis", result);
        }

        [TestMethod]
        public void MultipleTimeZoneNameMatches_NoResult()
        {
            // ARRANGE
            var googleTimeZone = new GoogleApiTimeZone
            {
                rawOffset = -18000,
                dstOffset = 3600,
                timeZoneName = "Bob"
            };

            // ACT
            var result = googleTimeZone.ToSalesforceTimeZone();

            // ASSERT
            Assert.AreEqual(string.Empty, result);
        }
    }
}