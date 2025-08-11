using FluentAssertions;
using M365ProxyAgent.Exceptions;
using M365ProxyAgent.Handlers;
using M365ProxyAgent.Interfaces;
using M365ProxyAgent.UnitTests.Builders;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace M365ProxyAgent.UnitTests.Handlers
{
    public class EventMessageHandlerTests
    {
        private const string TestCorrelationId = "test-correlation-id";
        private const string TestConversationId = "test-conversation-id";
        private const string TestUserId = "user-id";
        private const string TestUserName = "Test User";
        private const string TestEventName = "test-event";
        private const string TestResponseText = "Event processed successfully";
        private const string ExpectedTurnContextNullMessage = "Value cannot be null. (Parameter 'turnContext')";
        private const string ExpectedFailedProcessMessage = "Failed to process event message";
        private const string ExpectedProcessEventOperation = "ProcessEvent";

        private readonly Mock<IConversationService> _mockConversationService;
        private readonly Mock<ICorrelationService> _mockCorrelationService;
        private readonly Mock<ILogger<EventMessageHandler>> _mockLogger;
        private readonly Mock<ITurnContext> _mockTurnContext;
        private readonly Mock<ITurnState> _mockTurnState;

        public EventMessageHandlerTests()
        {
            _mockConversationService = new Mock<IConversationService>();
            _mockCorrelationService = new Mock<ICorrelationService>();
            _mockLogger = new Mock<ILogger<EventMessageHandler>>();
            _mockTurnContext = new Mock<ITurnContext>();
            _mockTurnState = new Mock<ITurnState>();

            _mockCorrelationService.Setup(cs => cs.CorrelationId).Returns(TestCorrelationId);
        }

        [Fact]
        public async Task HandleAsync_WithEventMessage_ProcessesEventSuccessfully()
        {
            // Arrange
            var activity = ActivityBuilder.CreateEventActivity(TestEventName);
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
        public async Task HandleAsync_WithMultipleResponseActivities_SendsAllActivities()
        {
            // Arrange
            var activity = ActivityBuilder.CreateEventActivity(TestEventName);
            _mockTurnContext.Setup(tc => tc.Activity).Returns(activity);

            var responseActivities = new List<Activity>
            {
                new Activity { Type = ActivityTypes.Message, Text = "First response" },
                new Activity { Type = ActivityTypes.Message, Text = "Second response" }
            };

            _mockConversationService.Setup(cs => cs.ProcessMessageAsync(activity, It.IsAny<CancellationToken>()))
                .Returns(CreateAsyncEnumerable(responseActivities));

            _mockTurnContext.Setup(tc => tc.SendActivityAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceResponse());

            var handler = CreateHandler();

            // Act
            await handler.HandleAsync(_mockTurnContext.Object, _mockTurnState.Object, CancellationToken.None);

            // Assert
            _mockTurnContext.Verify(tc => tc.SendActivityAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task HandleAsync_WithNullActivity_SkipsNullActivity()
        {
            // Arrange
            var activity = ActivityBuilder.CreateEventActivity(TestEventName);
            _mockTurnContext.Setup(tc => tc.Activity).Returns(activity);

            var responseActivities = new List<Activity?>
            {
                new Activity { Type = ActivityTypes.Message, Text = TestResponseText },
                null,
                new Activity { Type = ActivityTypes.Message, Text = "Second response" }
            };

            _mockConversationService.Setup(cs => cs.ProcessMessageAsync(activity, It.IsAny<CancellationToken>()))
                .Returns(CreateAsyncEnumerable(responseActivities.Where(a => a != null).Cast<Activity>()));

            _mockTurnContext.Setup(tc => tc.SendActivityAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceResponse());

            var handler = CreateHandler();

            // Act
            await handler.HandleAsync(_mockTurnContext.Object, _mockTurnState.Object, CancellationToken.None);

            // Assert
            _mockTurnContext.Verify(tc => tc.SendActivityAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
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
        public async Task HandleAsync_WithNullTurnState_ThrowsArgumentNullException()
        {
            // Arrange
            var handler = CreateHandler();

            // Act
            var act = async () => await handler.HandleAsync(_mockTurnContext.Object, null!, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task HandleAsync_WithConversationServiceFailure_ThrowsCopilotClientException()
        {
            // Arrange
            var activity = ActivityBuilder.CreateEventActivity(TestEventName);
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
            exception.Which.Operation.Should().Be(ExpectedProcessEventOperation);
            exception.Which.CorrelationId.Should().Be(TestCorrelationId);
        }

        [Fact]
        public async Task HandleAsync_WithProxyAgentException_DoesNotWrapException()
        {
            // Arrange
            var activity = ActivityBuilder.CreateEventActivity(TestEventName);
            _mockTurnContext.Setup(tc => tc.Activity).Returns(activity);

            var originalException = new CopilotClientException("Original copilot exception", "TestOperation", null, TestCorrelationId);
            _mockConversationService.Setup(cs => cs.ProcessMessageAsync(activity, It.IsAny<CancellationToken>()))
                .Throws(originalException);

            var handler = CreateHandler();

            // Act
            var act = async () => await handler.HandleAsync(_mockTurnContext.Object, _mockTurnState.Object, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<CopilotClientException>();
            exception.Which.Should().BeSameAs(originalException);
        }

        [Fact]
        public void CanHandle_WithEventActivityType_ReturnsTrue()
        {
            // Arrange
            var handler = CreateHandler();

            // Act
            var result = handler.CanHandle(ActivityTypes.Event);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanHandle_WithMessageActivityType_ReturnsFalse()
        {
            // Arrange
            var handler = CreateHandler();

            // Act
            var result = handler.CanHandle(ActivityTypes.Message);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanHandle_WithConversationUpdateActivityType_ReturnsFalse()
        {
            // Arrange
            var handler = CreateHandler();

            // Act
            var result = handler.CanHandle(ActivityTypes.ConversationUpdate);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithNullConversationService_ThrowsArgumentNullException()
        {
            // Act
            var act = () => new EventMessageHandler(null!, _mockCorrelationService.Object, _mockLogger.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("conversationService");
        }

        [Fact]
        public void Constructor_WithNullCorrelationService_ThrowsArgumentNullException()
        {
            // Act
            var act = () => new EventMessageHandler(_mockConversationService.Object, null!, _mockLogger.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("correlationService");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act
            var act = () => new EventMessageHandler(_mockConversationService.Object, _mockCorrelationService.Object, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        private EventMessageHandler CreateHandler()
        {
            return new EventMessageHandler(
                _mockConversationService.Object,
                _mockCorrelationService.Object,
                _mockLogger.Object);
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
