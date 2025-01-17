using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ApiAggregator.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace ApiAggregator.Tests.Middleware
{
    public class ExceptionMiddlewareTests
    {
        [Fact]
        public async Task Middleware_Returns500_OnException()
        {
            // Arrange
            var mockHttpContext = new DefaultHttpContext();
            var mockRequestDelegate = new Mock<RequestDelegate>();

            mockRequestDelegate
                .Setup(next => next(mockHttpContext))
                .Throws(new Exception("Test Exception"));

            var middleware = new ExceptionMiddleware(mockRequestDelegate.Object);

            var responseStream = new MemoryStream();
            mockHttpContext.Response.Body = responseStream;

            // Act
            await middleware.InvokeAsync(mockHttpContext);

            // Assert
            mockHttpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

            responseStream.Position = 0;
            var responseBody = await new StreamReader(responseStream).ReadToEndAsync();
            responseBody.Should().Contain("Test Exception");
        }
    }
}
