﻿using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AD_Users_Extract.Services;
using AD_Users_Extract.Services.Exceptions;
using AD_Users_Extract.Services.Interfaces;
using AD_Users_Extract.Services.Models;
using AD_Users_Extract.Tests.Stubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AD_Users_Extract.Tests
{
    [TestClass]
    public class GoogleServiceTests
    {
        [TestMethod]
        public async Task GeoCode_Success()
        {
            // ARRANGE
            var keyVaultServiceMock = new Mock<IAzureKeyVaultService>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var geoCodeResult = "{\"results\":[{\"address_components\":[{\"long_name\":\"68046\",\"short_name\":\"68046\",\"types\":[\"postal_code\"]},{\"long_name\":\"Papillion\",\"short_name\":\"Papillion\",\"types\":[\"locality\",\"political\"]},{\"long_name\":\"Nebraska\",\"short_name\":\"NE\",\"types\":[\"administrative_area_level_1\",\"political\"]},{\"long_name\":\"United States\",\"short_name\":\"US\",\"types\":[\"country\",\"political\"]}],\"formatted_address\":\"Papillion, NE 68046, USA\",\"geometry\":{\"bounds\":{\"northeast\":{\"lat\":41.1763281,\"lng\":-96.004504},\"southwest\":{\"lat\":41.053171,\"lng\":-96.11961079999999}},\"location\":{\"lat\":41.1319017,\"lng\":-96.0573302},\"location_type\":\"APPROXIMATE\",\"viewport\":{\"northeast\":{\"lat\":41.1763281,\"lng\":-96.004504},\"southwest\":{\"lat\":41.053171,\"lng\":-96.11961079999999}}},\"place_id\":\"ChIJn5ksLKiKk4cR42I8-mYyieY\",\"types\":[\"postal_code\"]}],\"status\":\"OK\"}";

            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(geoCodeResult, Encoding.UTF8)
                    };
                    return Task.FromResult(response);
                }
            );

            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new GoogleApiService(keyVaultServiceMock.Object, httpClientFactoryMock.Object);
            var apiKey = "APIKEY";

            keyVaultServiceMock.Setup(mock => mock.GetSecret("GoogleApiKey"))
                .ReturnsAsync(() => apiKey).Verifiable();

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>()))
                .Returns(client).Verifiable();

            // ACT
            var result = await unitUnderTest.GeoCode("825 Redwood Lane", "Papillion", "NE", "68046");

            // ASSERT
            keyVaultServiceMock.Verify();
            httpClientFactoryMock.Verify();
            Assert.AreEqual(41.1319017, result.lat);
            Assert.AreEqual(-96.0573302, result.lng);
            client.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedStatusCodeException))]
        public async Task GeoCode_NonSuccessStatusCode()
        {
            // ARRANGE
            var keyVaultServiceMock = new Mock<IAzureKeyVaultService>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();

            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.BadGateway);
                    return Task.FromResult(response);
                }
            );

            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new GoogleApiService(keyVaultServiceMock.Object, httpClientFactoryMock.Object);
            var apiKey = "APIKEY";

            keyVaultServiceMock.Setup(mock => mock.GetSecret("GoogleApiKey"))
                .ReturnsAsync(() => apiKey).Verifiable();

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>()))
                .Returns(client).Verifiable();

            // ACT
            var _ = await unitUnderTest.GeoCode("825 Redwood Lane", "Papillion", "NE", "68046");
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedDataException))]
        public async Task GeoCode_UnexpectedData()
        {
            // ARRANGE
            var keyVaultServiceMock = new Mock<IAzureKeyVaultService>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub();

            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new GoogleApiService(keyVaultServiceMock.Object, httpClientFactoryMock.Object);
            var apiKey = "APIKEY";

            keyVaultServiceMock.Setup(mock => mock.GetSecret("GoogleApiKey"))
                .ReturnsAsync(() => apiKey).Verifiable();

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>()))
                .Returns(client).Verifiable();

            // ACT
            var _ = await unitUnderTest.GeoCode("825 Redwood Lane", "Papillion", "NE", "68046");
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedDataException))]
        public async Task GeoCode_EmptyResults()
        {
            // ARRANGE
            var keyVaultServiceMock = new Mock<IAzureKeyVaultService>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var geoCodeResult = "{}";

            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(geoCodeResult, Encoding.UTF8)
                    };
                    return Task.FromResult(response);
                }
            );

            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new GoogleApiService(keyVaultServiceMock.Object, httpClientFactoryMock.Object);
            var apiKey = "APIKEY";

            keyVaultServiceMock.Setup(mock => mock.GetSecret("GoogleApiKey"))
                .ReturnsAsync(() => apiKey).Verifiable();

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>()))
                .Returns(client).Verifiable();

            // ACT
            var _ = await unitUnderTest.GeoCode("825 Redwood Lane", "Papillion", "NE", "68046");
        }

        [TestMethod]
        public async Task TimeZone_Success()
        {
            // ARRANGE
            var keyVaultServiceMock = new Mock<IAzureKeyVaultService>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var geoCodeResult = "{\"dstOffset\":3600,\"rawOffset\":-18000,\"status\":\"OK\",\"timeZoneId\":\"America/New_York\",\"timeZoneName\":\"Eastern Daylight Time\"}";

            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(geoCodeResult, Encoding.UTF8)
                    };
                    return Task.FromResult(response);
                }
            );

            var client = new HttpClient(clientHandlerStub);
            var unitUnderTest = new GoogleApiService(keyVaultServiceMock.Object, httpClientFactoryMock.Object);
            var apiKey = "APIKEY";

            keyVaultServiceMock.Setup(mock => mock.GetSecret("GoogleApiKey"))
                .ReturnsAsync(() => apiKey).Verifiable();

            httpClientFactoryMock.Setup(mock => mock.CreateClient(It.IsAny<string>()))
                .Returns(client).Verifiable();

            var location = new GoogleApiLocation
            {
                lat = 41.1319017, 
                lng = -96.0573302
            };

            // ACT
            var result = await unitUnderTest.TimeZone(location);

            // ASSERT
            keyVaultServiceMock.Verify();
            httpClientFactoryMock.Verify();
            Assert.AreEqual("America/New_York", result);
            client.Dispose();
        }
    }
}
