using FluentAssertions;
using M365ProxyAgent.UnitTests.Builders;
using Microsoft.Agents.Core.Models;

namespace M365ProxyAgent.UnitTests.Builders
{
    public class ActivityBuilderTests
    {
        [Fact]
        public void Build_WithDefaults_ReturnsActivityWithDefaultValues()
        {
            // Act
            var activity = new ActivityBuilder().Build();

            // Assert
            activity.Should().NotBeNull();
            activity.Type.Should().Be("message");
            activity.Id.Should().Be("test-activity-id");
            activity.ChannelId.Should().Be("test-channel");
            activity.From.Should().NotBeNull();
            activity.From.Id.Should().Be("test-user-id");
            activity.Conversation.Should().NotBeNull();
            activity.Conversation.Id.Should().Be("test-conversation-id");
        }

        [Fact]
        public void WithText_SetsTextProperty()
        {
            // Arrange
            const string expectedText = "Hello, World!";

            // Act
            var activity = new ActivityBuilder()
                .WithText(expectedText)
                .Build();

            // Assert
            activity.Text.Should().Be(expectedText);
        }

        [Fact]
        public void WithType_SetsActivityType()
        {
            // Arrange
            const string expectedType = "conversationUpdate";

            // Act
            var activity = new ActivityBuilder()
                .WithType(expectedType)
                .Build();

            // Assert
            activity.Type.Should().Be(expectedType);
        }

        [Fact]
        public void WithMembersAdded_SetsMembersAddedProperty()
        {
            // Arrange
            var member1 = new ChannelAccount { Id = "user1", Name = "User One" };
            var member2 = new ChannelAccount { Id = "user2", Name = "User Two" };

            // Act
            var activity = new ActivityBuilder()
                .WithMembersAdded(member1, member2)
                .Build();

            // Assert
            activity.MembersAdded.Should().NotBeNull();
            activity.MembersAdded.Should().HaveCount(2);
            activity.MembersAdded.Should().Contain(member1);
            activity.MembersAdded.Should().Contain(member2);
        }

        [Fact]
        public void CreateMessageActivity_ReturnsMessageActivity()
        {
            // Arrange
            const string expectedText = "Test message";

            // Act
            var activity = ActivityBuilder.CreateMessageActivity(expectedText);

            // Assert
            activity.Type.Should().Be("message");
            activity.Text.Should().Be(expectedText);
        }

        [Fact]
        public void CreateConversationUpdateActivity_ReturnsConversationUpdateActivity()
        {
            // Arrange
            var member = new ChannelAccount { Id = "newuser", Name = "New User" };

            // Act
            var activity = ActivityBuilder.CreateConversationUpdateActivity(member);

            // Assert
            activity.Type.Should().Be("conversationUpdate");
            activity.MembersAdded.Should().NotBeNull();
            activity.MembersAdded.Should().HaveCount(1);
            activity.MembersAdded.Should().Contain(member);
        }

        [Fact]
        public void CreateEmptyActivity_ReturnsActivityWithDefaults()
        {
            // Act
            var activity = ActivityBuilder.CreateEmptyActivity();

            // Assert
            activity.Should().NotBeNull();
            // Should have default initialization from the constructor
            activity.Type.Should().Be("message");
        }

        [Fact]
        public void FluentBuilder_CanChainMultipleMethods()
        {
            // Arrange
            const string expectedText = "Chained message";
            const string expectedType = "conversationUpdate";
            var member = new ChannelAccount { Id = "chainuser", Name = "Chain User" };

            // Act
            var activity = new ActivityBuilder()
                .WithType(expectedType)
                .WithText(expectedText)
                .WithMembersAdded(member)
                .Build();

            // Assert
            activity.Type.Should().Be(expectedType);
            activity.Text.Should().Be(expectedText);
            activity.MembersAdded.Should().NotBeNull();
            activity.MembersAdded.Should().HaveCount(1);
            activity.MembersAdded.Should().Contain(member);
        }
    }
}
