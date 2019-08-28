using System;
using System.Collections.Generic;
using Azure_AD_Users_Extract.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Azure_AD_Users_Extract.Tests
{
    [TestClass]
    public class ExceptionActionFilterTests
    {
        [TestMethod]
        public void OnException_Executes()
        {
            // ARRANGE
            var loggerMock = new Mock<ILogger<ExceptionActionFilter>>();
            var unitUnderTest = new ExceptionActionFilter(loggerMock.Object);
            var modelState = new ModelStateDictionary();
            var httpResponseMock = Mock.Of<HttpResponse>();
            var httpContextMock = new Mock<HttpContext>();

            httpContextMock.Setup(mock => mock.Response).Returns(httpResponseMock).Verifiable();

            var actionContext = new ActionContext(
                httpContextMock.Object,
                Mock.Of<RouteData>(),
                Mock.Of<ActionDescriptor>(),
                modelState
            );

            var exceptionContextContext = new ExceptionContext(actionContext, new List<IFilterMetadata>());

            // ACT
            unitUnderTest.OnException(exceptionContextContext);

            // ASSERT
            loggerMock.Verify(mock => mock.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<FormattedLogValues>(v => v.ToString().Contains("Unexpected error")),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
            httpContextMock.Verify();
            Assert.AreEqual(StatusCodes.Status500InternalServerError, httpResponseMock.StatusCode);
            Assert.AreEqual("application/json", httpResponseMock.ContentType);
        }
    }
}
