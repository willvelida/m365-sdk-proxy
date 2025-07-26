using FluentAssertions;
using M365ProxyAgent.Exceptions;
using M365ProxyAgent.Handlers;
using M365ProxyAgent.Interfaces;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace M365ProxyAgent.Tests.Handlers
{
    public class RegularMessageHandlerTests
    {
        private const string TestCorrelationId = "test-correlation-id";
        private const string TestConversationId = "test-conversation-id";
        private const string TestUserId = "user-id";
        private const string TestUserName = "Test User";
        private const string TestMessageText = "Hello, bot!";
        private const string TestResponseText = "Hello! How can I help you?";
        private const string ExpectedTurnContextNullMessage = "Value cannot be null. (Parameter 'turnContext')";
        private const string ExpectedFailedProcessMessage = "Failed to process user message";
        private const string ExpectedProcessMessageOperation = "ProcessMessage";

        private readonly Mock<IConversationService> _mockConversationService;
        private readonly Mock<ICorrelationService> _mockCorrelationService;
        private readonly Mock<ILogger<RegularMessageHandler>> _mockLogger;
        private readonly Mock<ITurnContext> _mockTurnContext;
        private readonly Mock<ITurnState> _mockTurnState;

        public RegularMessageHandlerTests()
        {
            _mockConversationService = new Mock<IConversationService>();
            _mockCorrelationService = new Mock<ICorrelationService>();
            _mockLogger = new Mock<ILogger<RegularMessageHandler>>();
            _mockTurnContext = new Mock<ITurnContext>();
            _mockTurnState = new Mock<ITurnState>();

            _mockCorrelationService.Setup(cs => cs.CorrelationId).Returns(TestCorrelationId);
        }

        [Fact]
        public async Task HandleAsync_WithUserMessage_ProcessesMessageSuccessfully()
        {
            // Arrange
            var activity = CreateMessageActivity();
            _mockTurnContext.Setup(tc => tc.Activity).Returns(activity);

            var responseActivities = new List<Activity>
            {
                new Activity { Type = ActivityTypes.Message, Text = TestResponseText }
            };

            _mockConversationService.Setup(cs => cs.ProcessMessageAsync(activity, It.IsAny<CancellationToken>()))
                .Returns(CreateAsyncEnumerable(responseActivities));

            _mockTurnContext.Setup(tc => tc.SendActivityAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceResponse());

            var handler = CreateHandler();

            // Act
            await handler.HandleAsync(_mockTurnContext.Object, _mockTurnState.Object, CancellationToken.None);

            // Assert
            _mockConversationService.Verify(cs => cs.ProcessMessageAsync(activity, It.IsAny<CancellationToken>()), Times.Once);
            _mockTurnContext.Verify(tc => tc.SendActivityAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WithNullTurnContext_ThrowsArgumentNullException()
        {
            // Arrange
            var handler = CreateHandler();

            // Act
            var act = async () => await handler.HandleAsync(null!, _mockTurnState.Object, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage(ExpectedTurnContextNullMessage);
        }

        [Fact]
        public async Task HandleAsync_WithConversationServiceFailure_ThrowsCopilotClientException()
        {
            // Arrange
            var activity = CreateMessageActivity();
            _mockTurnContext.Setup(tc => tc.Activity).Returns(activity);

            var originalException = new InvalidOperationException("Service unavailable");
            _mockConversationService.Setup(cs => cs.ProcessMessageAsync(activity, It.IsAny<CancellationToken>()))
                .Throws(originalException);

            var handler = CreateHandler();

            // Act
            var act = async () => await handler.HandleAsync(_mockTurnContext.Object, _mockTurnState.Object, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<CopilotClientException>()
                .WithMessage(ExpectedFailedProcessMessage);
            
            exception.Which.InnerException.Should().Be(originalException);
            exception.Which.Operation.Should().Be(ExpectedProcessMessageOperation);
            exception.Which.CorrelationId.Should().Be(TestCorrelationId);
        }

        private RegularMessageHandler CreateHandler()
        {
            return new RegularMessageHandler(
                _mockConversationService.Object,
                _mockCorrelationService.Object,
                _mockLogger.Object);
        }

        private static Activity CreateMessageActivity()
        {
            return new Activity
            {
                Type = ActivityTypes.Message,
                Text = TestMessageText,
                Conversation = new ConversationAccount { Id = TestConversationId },
                From = new ChannelAccount { Id = TestUserId, Name = TestUserName }
            };
        }

        private static async IAsyncEnumerable<Activity> CreateAsyncEnumerable(IEnumerable<Activity> activities)
        {
            foreach (var activity in activities)
            {
                yield return activity;
            }
            await Task.CompletedTask;
        }
    }
}
