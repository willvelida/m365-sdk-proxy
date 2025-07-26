using FluentAssertions;
using M365ProxyAgent.Interfaces;
using M365ProxyAgent.Services;
using Microsoft.Agents.Builder;
using Microsoft.Agents.CopilotStudio.Client;
using Microsoft.Agents.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace M365ProxyAgent.UnitTests.Services
{
    public class ConversationServiceTests
    {
        private const string TurnContextParameterName = "turnContext";
        private const string UserActivityParameterName = "userActivity";
        private const string CopilotClientParameterName = "copilotClient";
        private const string ExpectedTurnContextNullMessage = "Value cannot be null. (Parameter 'turnContext')";
        private const string ExpectedUserActivityNullMessage = "Value cannot be null. (Parameter 'userActivity')";
        private const string ExpectedCopilotClientNullMessage = "Value cannot be null. (Parameter 'copilotClient')";
        
        private readonly Mock<ICorrelationService> _mockCorrelationService;
        private readonly Mock<IResilienceService> _mockResilienceService;
        private readonly Mock<ILogger<ConversationService>> _mockLogger;

        public ConversationServiceTests()
        {
            _mockCorrelationService = new Mock<ICorrelationService>();
            _mockResilienceService = new Mock<IResilienceService>();
            _mockLogger = new Mock<ILogger<ConversationService>>();
        }

        [Fact]
        public void StartConversationAsync_WithNullTurnContext_ThrowsArgumentNullException()
        {
            // Arrange
            var mockCopilotClient = CreateMockCopilotClient();
            var service = CreateConversationService(mockCopilotClient);

            // Act & Assert
            var act = async () =>
            {
                await foreach (var activity in service.StartConversationAsync(null!, CancellationToken.None))
                {
                    // This should not execute
                }
            };

            act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage(ExpectedTurnContextNullMessage);
        }

        [Fact]
        public void ProcessMessageAsync_WithNullUserActivity_ThrowsArgumentNullException()
        {
            // Arrange
            var mockCopilotClient = CreateMockCopilotClient();
            var service = CreateConversationService(mockCopilotClient);

            // Act & Assert
            var act = async () =>
            {
                await foreach (var activity in service.ProcessMessageAsync(null!, CancellationToken.None))
                {
                    // This should not execute
                }
            };

            act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage(ExpectedUserActivityNullMessage);
        }

        [Fact]
        public void Constructor_WithNullCopilotClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => CreateConversationService(null!);

            act.Should().Throw<ArgumentNullException>()
                .WithMessage(ExpectedCopilotClientNullMessage);
        }

        private ConversationService CreateConversationService(CopilotClient copilotClient)
        {
            return new ConversationService(
                copilotClient,
                _mockCorrelationService.Object,
                _mockResilienceService.Object,
                _mockLogger.Object);
        }

        private static CopilotClient CreateMockCopilotClient()
        {
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockLogger = new Mock<ILogger<CopilotClient>>();
            var mockSettings = new Mock<Microsoft.Agents.CopilotStudio.Client.ConnectionSettings>();
            
            return new CopilotClient(mockSettings.Object, mockHttpClientFactory.Object, mockLogger.Object, "test");
        }
    }
}
