using FluentAssertions;
using M365ProxyAgent.Agents;
using M365ProxyAgent.Interfaces;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace M365ProxyAgent.UnitTests.Agents
{
    public class ProxyAgentTests
    {
        private const string TestConversationId = "test-conversation-id";
        private const string TestUserId = "test-user-id";
        private const string TestMessageText = "Hello, ProxyAgent!";
        
        private readonly Mock<IMessageHandlerFactory> _mockMessageHandlerFactory;
        private readonly Mock<ILogger<ProxyAgent>> _mockLogger;

        public ProxyAgentTests()
        {
            _mockMessageHandlerFactory = new Mock<IMessageHandlerFactory>();
            _mockLogger = new Mock<ILogger<ProxyAgent>>();
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            var act = () => new ProxyAgent(null!, _mockMessageHandlerFactory.Object, _mockLogger.Object);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("options");
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            var act = () => new ProxyAgent(null!, _mockMessageHandlerFactory.Object, null!);

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
