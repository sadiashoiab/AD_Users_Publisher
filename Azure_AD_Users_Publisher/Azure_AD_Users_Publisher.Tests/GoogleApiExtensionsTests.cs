using System.Linq;
using Azure_AD_Users_Publisher.Services;
using Azure_AD_Users_Publisher.Services.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Azure_AD_Users_Publisher.Tests
{
    [TestClass]
    public class GoogleApiExtensionsTests
    {
        [TestMethod]
        public void OneMatches()
        {
            // ARRANGE
            // new SalesforceSupportedTimeZone(-36000, "Hawaii-Aleutian Standard Time", "Pacific/Honolulu"),
            var googleTimeZone = new GoogleApiTimeZoneResult
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
            var googleTimeZone = new GoogleApiTimeZoneResult
            {
                rawOffset = -18000,
                dstOffset = 3600,
                timeZoneName = "Eastern Daylight Time"
            };

            // ACT
            var result = googleTimeZone.ToSalesforceTimeZone();

            // ASSERT
            Assert.AreEqual("America/New_York", result);
        }

        [TestMethod]
        public void MultipleTimeZoneNameMatches_NoResult()
        {
            // ARRANGE
            var googleTimeZone = new GoogleApiTimeZoneResult
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

        
        [TestMethod]
        public void AddressComponentsTest_Success()
        {
            var json = "{\"results\":[{\"address_components\":[{\"long_name\":\"1600\",\"short_name\":\"1600\",\"types\":[\"street_number\"]},{\"long_name\":\"Amphitheatre Pkwy\",\"short_name\":\"Amphitheatre Pkwy\",\"types\":[\"route\"]},{\"long_name\":\"Mountain View\",\"short_name\":\"Mountain View\",\"types\":[\"locality\",\"political\"]},{\"long_name\":\"Santa Clara County\",\"short_name\":\"Santa Clara County\",\"types\":[\"administrative_area_level_2\",\"political\"]},{\"long_name\":\"California\",\"short_name\":\"CA\",\"types\":[\"administrative_area_level_1\",\"political\"]},{\"long_name\":\"United States\",\"short_name\":\"US\",\"types\":[\"country\",\"political\"]},{\"long_name\":\"94043\",\"short_name\":\"94043\",\"types\":[\"postal_code\"]}],\"formatted_address\":\"1600 Amphitheatre Parkway, Mountain View, CA 94043, USA\",\"geometry\":{\"location\":{\"lat\":37.4224764,\"lng\":-122.0842499},\"location_type\":\"ROOFTOP\",\"viewport\":{\"northeast\":{\"lat\":37.4238253802915,\"lng\":-122.0829009197085},\"southwest\":{\"lat\":37.4211274197085,\"lng\":-122.0855988802915}}},\"place_id\":\"ChIJ2eUgeAK6j4ARbn5u_wAGqWA\",\"types\":[\"street_address\"]}],\"status\":\"OK\"}";
            var googleRootObject = System.Text.Json.JsonSerializer.Deserialize<GoogleApiGeoCodeRootObject>(json);
            var googleLocation = googleRootObject.results.FirstOrDefault();
            var countryCode = googleLocation.ToCountryCode();

            Assert.AreEqual("US", countryCode);
        }

        [TestMethod]
        public void AddressComponentsTest_NoResult()
        {
            var json = "{\"results\":[{\"address_components\":[{\"long_name\":\"1600\",\"short_name\":\"1600\",\"types\":[\"street_number\"]},{\"long_name\":\"Amphitheatre Pkwy\",\"short_name\":\"Amphitheatre Pkwy\",\"types\":[\"route\"]},{\"long_name\":\"Mountain View\",\"short_name\":\"Mountain View\",\"types\":[\"locality\",\"political\"]},{\"long_name\":\"Santa Clara County\",\"short_name\":\"Santa Clara County\",\"types\":[\"administrative_area_level_2\",\"political\"]},{\"long_name\":\"California\",\"short_name\":\"CA\",\"types\":[\"administrative_area_level_1\",\"political\"]},{\"long_name\":\"94043\",\"short_name\":\"94043\",\"types\":[\"postal_code\"]}],\"formatted_address\":\"1600 Amphitheatre Parkway, Mountain View, CA 94043, USA\",\"geometry\":{\"location\":{\"lat\":37.4224764,\"lng\":-122.0842499},\"location_type\":\"ROOFTOP\",\"viewport\":{\"northeast\":{\"lat\":37.4238253802915,\"lng\":-122.0829009197085},\"southwest\":{\"lat\":37.4211274197085,\"lng\":-122.0855988802915}}},\"place_id\":\"ChIJ2eUgeAK6j4ARbn5u_wAGqWA\",\"types\":[\"street_address\"]}],\"status\":\"OK\"}";
            var googleRootObject = System.Text.Json.JsonSerializer.Deserialize<GoogleApiGeoCodeRootObject>(json);
            var googleLocation = googleRootObject.results.FirstOrDefault();
            var countryCode = googleLocation.ToCountryCode();

            Assert.IsNull(countryCode);
        }
    }
}