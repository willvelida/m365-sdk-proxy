using FluentAssertions;
using M365ProxyAgent.Exceptions;
using M365ProxyAgent.Interfaces;
using M365ProxyAgent.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace M365ProxyAgent.UnitTests.Middleware
{
    public class ExceptionHandlingMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _mockNext;
        private readonly Mock<ICorrelationService> _mockCorrelationService;
        private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _mockLogger;

        private const string TestCorrelationId = "test-correlation-id";

        public ExceptionHandlingMiddlewareTests()
        {
            _mockNext = new Mock<RequestDelegate>();
            _mockCorrelationService = new Mock<ICorrelationService>();
            _mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        }

        [Fact]
        public async Task InvokeAsync_WithNoException_CallsNextDelegate()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ICorrelationService)))
                              .Returns(_mockCorrelationService.Object);

            var context = new DefaultHttpContext();
            context.RequestServices = mockServiceProvider.Object;

            var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _mockNext.Verify(next => next(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithProxyAgentException_Returns500StatusWithErrorResponse()
        {
            // Arrange
            var exceptionMessage = "Test configuration error";
            var exception = new ConfigurationException(exceptionMessage);
            
            _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);
            _mockCorrelationService.Setup(cs => cs.CorrelationId).Returns(TestCorrelationId);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ICorrelationService)))
                              .Returns(_mockCorrelationService.Object);

            var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
            var context = new DefaultHttpContext();
            context.RequestServices = mockServiceProvider.Object;
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(500);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            errorResponse.GetProperty("correlationId").GetString().Should().Be(TestCorrelationId);
            errorResponse.GetProperty("error").GetString().Should().Be("ConfigurationError");
        }

        [Fact]
        public async Task InvokeAsync_WithAuthenticationException_Returns401StatusWithErrorResponse()
        {
            // Arrange
            var exceptionMessage = "Token validation failed";
            var exception = new AuthenticationException(exceptionMessage);
            
            _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);
            _mockCorrelationService.Setup(cs => cs.CorrelationId).Returns(TestCorrelationId);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ICorrelationService)))
                              .Returns(_mockCorrelationService.Object);

            var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
            var context = new DefaultHttpContext();
            context.RequestServices = mockServiceProvider.Object;
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(401);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            errorResponse.GetProperty("correlationId").GetString().Should().Be(TestCorrelationId);
            errorResponse.GetProperty("error").GetString().Should().Be("AuthenticationError");
        }

        [Fact]
        public async Task InvokeAsync_WithCopilotClientException_Returns502StatusWithErrorResponse()
        {
            // Arrange
            var exceptionMessage = "Failed to communicate with Copilot Studio";
            var exception = new CopilotClientException(exceptionMessage);
            
            _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);
            _mockCorrelationService.Setup(cs => cs.CorrelationId).Returns(TestCorrelationId);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ICorrelationService)))
                              .Returns(_mockCorrelationService.Object);

            var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
            var context = new DefaultHttpContext();
            context.RequestServices = mockServiceProvider.Object;
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(502);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            errorResponse.GetProperty("correlationId").GetString().Should().Be(TestCorrelationId);
            errorResponse.GetProperty("error").GetString().Should().Be("CopilotServiceError");
        }

        [Fact]
        public async Task InvokeAsync_WithValidationException_Returns400StatusWithErrorResponse()
        {
            // Arrange
            var exceptionMessage = "Invalid request data";
            var exception = new ValidationException(exceptionMessage);
            
            _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);
            _mockCorrelationService.Setup(cs => cs.CorrelationId).Returns(TestCorrelationId);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ICorrelationService)))
                              .Returns(_mockCorrelationService.Object);

            var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
            var context = new DefaultHttpContext();
            context.RequestServices = mockServiceProvider.Object;
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(400);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            errorResponse.GetProperty("correlationId").GetString().Should().Be(TestCorrelationId);
            errorResponse.GetProperty("error").GetString().Should().Be("ValidationError");
        }

        [Fact]
        public async Task InvokeAsync_WithGenericException_Returns500StatusWithErrorResponse()
        {
            // Arrange
            var exceptionMessage = "An unexpected error occurred";
            var exception = new InvalidOperationException(exceptionMessage);
            
            _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);
            _mockCorrelationService.Setup(cs => cs.CorrelationId).Returns(TestCorrelationId);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ICorrelationService)))
                              .Returns(_mockCorrelationService.Object);

            var middleware = new ExceptionHandlingMiddleware(_mockNext.Object, _mockLogger.Object);
            var context = new DefaultHttpContext();
            context.RequestServices = mockServiceProvider.Object;
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(500);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            errorResponse.GetProperty("correlationId").GetString().Should().Be(TestCorrelationId);
            errorResponse.GetProperty("error").GetString().Should().Be("InternalServerError");
            errorResponse.GetProperty("message").GetString().Should().Be("An unexpected error occurred. Please contact support.");
        }
    }
}
