using Microsoft.Agents.Core.Models;

namespace M365ProxyAgent.Tests.Builders
{
    public class ActivityBuilder
    {
        private readonly Activity _activity;

        public ActivityBuilder()
        {
            _activity = new Activity
            {
                Type = ActivityTypes.Message,
                Id = "test-activity-id",
                Timestamp = DateTimeOffset.UtcNow,
                ChannelId = "test-channel",
                ServiceUrl = "https://test.botframework.com",
                From = new ChannelAccount
                {
                    Id = "test-user-id",
                    Name = "Test User"
                },
                Conversation = new ConversationAccount
                {
                    Id = "test-conversation-id",
                    Name = "Test Conversation"
                },
                Recipient = new ChannelAccount
                {
                    Id = "test-bot-id",
                    Name = "Test Bot"
                }
            };
        }

        public ActivityBuilder WithType(string activityType)
        {
            _activity.Type = activityType;
            return this;
        }

        public ActivityBuilder WithText(string text)
        {
            _activity.Text = text;
            return this;
        }

        public ActivityBuilder WithMembersAdded(params ChannelAccount[] members)
        {
            _activity.MembersAdded = members.ToList();
            return this;
        }

        public Activity Build() => _activity;

        public static Activity CreateMessageActivity(string text = "Test message")
        {
            return new ActivityBuilder()
                .WithType(ActivityTypes.Message)
                .WithText(text)
                .Build();
        }

        public static Activity CreateConversationUpdateActivity(ChannelAccount member)
        {
            return new ActivityBuilder()
                .WithType(ActivityTypes.ConversationUpdate)
                .WithMembersAdded(member)
                .Build();
        }

        public static Activity CreateEmptyActivity()
        {
            return new ActivityBuilder().Build();
        }
    }
}
