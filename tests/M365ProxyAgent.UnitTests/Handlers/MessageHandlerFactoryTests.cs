using FluentAssertions;
using M365ProxyAgent.Handlers;
using M365ProxyAgent.Interfaces;
using Microsoft.Agents.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace M365ProxyAgent.UnitTests.Handlers
{
    public class MessageHandlerFactoryTests
    {
        private const string ExpectedArgumentNullExceptionMessage = "Value cannot be null. (Parameter 'activityType')";
        
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly MessageHandlerFactory _factory;

        public MessageHandlerFactoryTests()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();
            _factory = new MessageHandlerFactory(_mockServiceProvider.Object);
        }

        [Fact]
        public void CreateHandler_WithConversationUpdateActivity_ReturnsWelcomeMessageHandler()
        {
            // Arrange
            var mockWelcomeHandler = CreateMockWelcomeMessageHandler();
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(WelcomeMessageHandler)))
                .Returns(mockWelcomeHandler.Object);

            // Act
            var result = _factory.CreateHandler(ActivityTypes.ConversationUpdate);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<IMessageHandler>();
            _mockServiceProvider.Verify(sp => sp.GetService(typeof(WelcomeMessageHandler)), Times.Once);
        }

        [Fact]
        public void CreateHandler_WithMessageActivity_ReturnsRegularMessageHandler()
        {
            // Arrange
            var mockRegularHandler = CreateMockRegularMessageHandler();
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(RegularMessageHandler)))
                .Returns(mockRegularHandler.Object);

            // Act
            var result = _factory.CreateHandler(ActivityTypes.Message);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<IMessageHandler>();
            _mockServiceProvider.Verify(sp => sp.GetService(typeof(RegularMessageHandler)), Times.Once);
        }

        [Fact]
        public void CreateHandler_WithUnknownActivityType_ReturnsNull()
        {
            // Act
            var result = _factory.CreateHandler(ActivityTypes.Typing);

            // Assert
            result.Should().BeNull();
            _mockServiceProvider.Verify(sp => sp.GetService(It.IsAny<Type>()), Times.Never);
        }

        [Fact]
        public void CreateHandler_WithNullActivityType_ThrowsArgumentNullException()
        {
            // Act
            var act = () => _factory.CreateHandler(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage(ExpectedArgumentNullExceptionMessage);
            _mockServiceProvider.Verify(sp => sp.GetService(It.IsAny<Type>()), Times.Never);
        }

        private static Mock<WelcomeMessageHandler> CreateMockWelcomeMessageHandler()
        {
            return new Mock<WelcomeMessageHandler>(
                Mock.Of<IConversationService>(),
                Mock.Of<ICorrelationService>(),
                Mock.Of<ILogger<WelcomeMessageHandler>>());
        }

        private static Mock<RegularMessageHandler> CreateMockRegularMessageHandler()
        {
            return new Mock<RegularMessageHandler>(
                Mock.Of<IConversationService>(),
                Mock.Of<ICorrelationService>(),
                Mock.Of<ILogger<RegularMessageHandler>>());
        }
    }
}
