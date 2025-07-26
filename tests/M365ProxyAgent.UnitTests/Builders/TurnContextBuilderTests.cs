using FluentAssertions;
using M365ProxyAgent.UnitTests.Builders;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Core.Models;
using Moq;

namespace M365ProxyAgent.UnitTests.Builders
{
    public class TurnContextBuilderTests
    {
        [Fact]
        public void Build_WithDefaults_ReturnsTurnContextWithDefaultValues()
        {
            // Act
            var turnContext = new TurnContextBuilder().Build();

            // Assert
            turnContext.Should().NotBeNull();
            turnContext.Activity.Should().NotBeNull();
            turnContext.Activity.Type.Should().Be("message");
            turnContext.Activity.Text.Should().Be("Default test message");
            turnContext.Responded.Should().BeFalse();
        }

        [Fact]
        public void WithActivity_SetsActivityProperty()
        {
            // Arrange
            var expectedActivity = ActivityBuilder.CreateMessageActivity("Custom message");

            // Act
            var turnContext = new TurnContextBuilder()
                .WithActivity(expectedActivity)
                .Build();

            // Assert
            turnContext.Activity.Should().Be(expectedActivity);
            turnContext.Activity.Text.Should().Be("Custom message");
        }

        [Fact]
        public void WithActivity_UsingActivityBuilder_SetsActivityFromBuilder()
        {
            // Arrange
            var activityBuilder = new ActivityBuilder()
                .WithText("Builder message")
                .WithType("conversationUpdate");

            // Act
            var turnContext = new TurnContextBuilder()
                .WithActivity(activityBuilder)
                .Build();

            // Assert
            turnContext.Activity.Text.Should().Be("Builder message");
            turnContext.Activity.Type.Should().Be("conversationUpdate");
        }

        [Fact]
        public void WithResponded_SetsRespondedProperty()
        {
            // Act
            var turnContext = new TurnContextBuilder()
                .WithResponded(true)
                .Build();

            // Assert
            turnContext.Responded.Should().BeTrue();
        }

        [Fact]
        public async Task WithSendActivityResponse_ConfiguresSendActivityAsync()
        {
            // Arrange
            var expectedResponse = new ResourceResponse { Id = "custom-response-id" };
            var testActivity = ActivityBuilder.CreateMessageActivity("Response test");

            // Act
            var turnContext = new TurnContextBuilder()
                .WithSendActivityResponse(expectedResponse)
                .Build();

            var result = await turnContext.SendActivityAsync(testActivity);

            // Assert
            result.Should().Be(expectedResponse);
            result.Id.Should().Be("custom-response-id");
            turnContext.Responded.Should().BeTrue();
        }

        [Fact]
        public async Task WithSendActivityResponse_DefaultResponse_UsesDefaultResourceResponse()
        {
            // Arrange
            var testActivity = ActivityBuilder.CreateMessageActivity("Default response test");

            // Act
            var turnContext = new TurnContextBuilder()
                .WithSendActivityResponse()
                .Build();

            var result = await turnContext.SendActivityAsync(testActivity);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("response-123");
            turnContext.Responded.Should().BeTrue();
        }

        [Fact]
        public void GetMock_ReturnsUnderlyingMock()
        {
            // Arrange
            var builder = new TurnContextBuilder();

            // Act
            var mock = builder.GetMock();

            // Assert
            mock.Should().NotBeNull();
            mock.Should().BeOfType<Mock<ITurnContext>>();
        }

        [Fact]
        public void CreateMessageTurnContext_ReturnsMessageTurnContext()
        {
            // Arrange
            const string expectedMessage = "Factory message";

            // Act
            var turnContext = TurnContextBuilder.CreateMessageTurnContext(expectedMessage);

            // Assert
            turnContext.Activity.Type.Should().Be("message");
            turnContext.Activity.Text.Should().Be(expectedMessage);
        }

        [Fact]
        public void CreateMessageTurnContext_DefaultMessage_UsesDefaultMessage()
        {
            // Act
            var turnContext = TurnContextBuilder.CreateMessageTurnContext();

            // Assert
            turnContext.Activity.Type.Should().Be("message");
            turnContext.Activity.Text.Should().Be("Test message");
        }

        [Fact]
        public void CreateConversationUpdateTurnContext_ReturnsConversationUpdateTurnContext()
        {
            // Arrange
            var member = new ChannelAccount { Id = "factory-user", Name = "Factory User" };

            // Act
            var turnContext = TurnContextBuilder.CreateConversationUpdateTurnContext(member);

            // Assert
            turnContext.Activity.Type.Should().Be("conversationUpdate");
            turnContext.Activity.MembersAdded.Should().Contain(member);
        }

        [Fact]
        public void CreateConversationUpdateTurnContext_DefaultMember_UsesDefaultMember()
        {
            // Act
            var turnContext = TurnContextBuilder.CreateConversationUpdateTurnContext();

            // Assert
            turnContext.Activity.Type.Should().Be("conversationUpdate");
            turnContext.Activity.MembersAdded.Should().NotBeEmpty();
            turnContext.Activity.MembersAdded.First().Id.Should().Be("test-user");
        }

        [Fact]
        public void CreateRespondedTurnContext_ReturnsRespondedTurnContext()
        {
            // Arrange
            const string expectedMessage = "Already responded message";

            // Act
            var turnContext = TurnContextBuilder.CreateRespondedTurnContext(expectedMessage);

            // Assert
            turnContext.Activity.Text.Should().Be(expectedMessage);
            turnContext.Responded.Should().BeTrue();
        }

        [Fact]
        public void FluentBuilder_CanChainMultipleMethods()
        {
            // Arrange
            var customActivity = ActivityBuilder.CreateMessageActivity("Chained message");

            // Act
            var turnContext = new TurnContextBuilder()
                .WithActivity(customActivity)
                .WithResponded(true)
                .Build();

            // Assert
            turnContext.Activity.Should().Be(customActivity);
            turnContext.Responded.Should().BeTrue();
        }
    }
}
