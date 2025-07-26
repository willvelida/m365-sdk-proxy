using FluentAssertions;
using M365ProxyAgent.Exceptions;
using M365ProxyAgent.Handlers;
using M365ProxyAgent.Interfaces;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace M365ProxyAgent.UnitTests.Handlers
{
    public class WelcomeMessageHandlerTests
    {
        private const string TestCorrelationId = "test-correlation-id";
        private const string TestConversationId = "test-conversation-id";
        private const string TestNewMemberId = "new-member-id";
        private const string TestRecipientId = "bot-id";
        private const string TestMemberName = "New Member";

        private readonly Mock<IConversationService> _mockConversationService;
        private readonly Mock<ICorrelationService> _mockCorrelationService;
        private readonly Mock<ILogger<WelcomeMessageHandler>> _mockLogger;
        private readonly Mock<ITurnContext> _mockTurnContext;
        private readonly Mock<ITurnState> _mockTurnState;

        public WelcomeMessageHandlerTests()
        {
            _mockConversationService = new Mock<IConversationService>();
            _mockCorrelationService = new Mock<ICorrelationService>();
            _mockLogger = new Mock<ILogger<WelcomeMessageHandler>>();
            _mockTurnContext = new Mock<ITurnContext>();
            _mockTurnState = new Mock<ITurnState>();

            _mockCorrelationService.Setup(cs => cs.CorrelationId).Returns(TestCorrelationId);
        }

        [Fact]
        public async Task HandleAsync_WithMembersAdded_ProcessesWelcomeMessage()
        {
            // Arrange
            var activity = CreateConversationUpdateActivity();
            _mockTurnContext.Setup(tc => tc.Activity).Returns(activity);

            var welcomeActivities = new List<Activity>
            {
                new Activity { Type = ActivityTypes.Message, Text = "Welcome!" }
            };

            _mockConversationService.Setup(cs => cs.StartConversationAsync(_mockTurnContext.Object, It.IsAny<CancellationToken>()))
                .Returns(CreateAsyncEnumerable(welcomeActivities));

            _mockTurnContext.Setup(tc => tc.SendActivityAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceResponse());

            var handler = CreateHandler();

            // Act
            await handler.HandleAsync(_mockTurnContext.Object, _mockTurnState.Object, CancellationToken.None);

            // Assert
            _mockConversationService.Verify(cs => cs.StartConversationAsync(_mockTurnContext.Object, It.IsAny<CancellationToken>()), Times.Once);
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
                .WithMessage("Value cannot be null. (Parameter 'turnContext')");
        }

        [Fact]
        public async Task HandleAsync_WithConversationServiceFailure_ThrowsCopilotClientException()
        {
            // Arrange
            var activity = CreateConversationUpdateActivity();
            _mockTurnContext.Setup(tc => tc.Activity).Returns(activity);

            var originalException = new InvalidOperationException("Service unavailable");
            _mockConversationService.Setup(cs => cs.StartConversationAsync(_mockTurnContext.Object, It.IsAny<CancellationToken>()))
                .Throws(originalException);

            var handler = CreateHandler();

            // Act
            var act = async () => await handler.HandleAsync(_mockTurnContext.Object, _mockTurnState.Object, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<CopilotClientException>()
                .WithMessage("Failed to process welcome message");
            
            exception.Which.InnerException.Should().Be(originalException);
            exception.Which.Operation.Should().Be("WelcomeMessage");
            exception.Which.CorrelationId.Should().Be(TestCorrelationId);
        }

        private WelcomeMessageHandler CreateHandler()
        {
            return new WelcomeMessageHandler(
                _mockConversationService.Object,
                _mockCorrelationService.Object,
                _mockLogger.Object);
        }

        private static Activity CreateConversationUpdateActivity()
        {
            return new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                Conversation = new ConversationAccount { Id = TestConversationId },
                Recipient = new ChannelAccount { Id = TestRecipientId },
                MembersAdded = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = TestNewMemberId, Name = TestMemberName }
                }
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
