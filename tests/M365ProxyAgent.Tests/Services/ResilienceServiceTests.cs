using FluentAssertions;
using M365ProxyAgent.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Polly;

namespace M365ProxyAgent.Tests.Services
{
    public class ResilienceServiceTests
    {
        private readonly Mock<ILogger<ResilienceService>> _mockLogger;
        private readonly ResilienceService _service;

        public ResilienceServiceTests()
        {
            _mockLogger = new Mock<ILogger<ResilienceService>>();
            _service = new ResilienceService(_mockLogger.Object);
        }

        [Fact]
        public void AuthenticationPipeline_WhenAccessed_ReturnsConfiguredPipeline()
        {
            // Act
            var pipeline = _service.AuthenticationPipeline;

            // Assert
            ValidateResiliencePipeline(pipeline);
        }

        [Fact]
        public void CopilotClientPipeline_WhenAccessed_ReturnsConfiguredPipeline()
        {
            // Act
            var pipeline = _service.CopilotClientPipeline;

            // Assert
            ValidateResiliencePipeline(pipeline);
        }

        [Fact]
        public void HttpPipeline_WhenAccessed_ReturnsConfiguredPipeline()
        {
            // Act
            var pipeline = _service.HttpPipeline;

            // Assert
            ValidateResiliencePipeline(pipeline);
        }

        private static void ValidateResiliencePipeline(ResiliencePipeline pipeline)
        {
            pipeline.Should().NotBeNull();
            pipeline.Should().BeOfType<ResiliencePipeline>();
        }
    }
}
