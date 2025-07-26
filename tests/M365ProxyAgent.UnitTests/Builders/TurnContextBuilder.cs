using Microsoft.Agents.Builder;
using Microsoft.Agents.Core.Models;
using Moq;

namespace M365ProxyAgent.UnitTests.Builders
{
    public class TurnContextBuilder
    {
        private Activity _activity;
        private Mock<ITurnContext> _mockTurnContext;
        private bool _responded = false;

        public TurnContextBuilder()
        {
            _activity = ActivityBuilder.CreateMessageActivity("Default test message");
            _mockTurnContext = new Mock<ITurnContext>();
            SetupDefaultMockBehavior();
        }

        public TurnContextBuilder WithActivity(Activity activity)
        {
            _activity = activity;
            _mockTurnContext.Setup(x => x.Activity).Returns(_activity);
            return this;
        }

        public TurnContextBuilder WithActivity(ActivityBuilder activityBuilder)
        {
            return WithActivity(activityBuilder.Build());
        }

        public TurnContextBuilder WithResponded(bool responded)
        {
            _responded = responded;
            _mockTurnContext.Setup(x => x.Responded).Returns(_responded);
            return this;
        }

        public TurnContextBuilder WithSendActivityResponse(ResourceResponse? response = null)
        {
            response ??= new ResourceResponse { Id = "response-123" };
            _mockTurnContext
                .Setup(x => x.SendActivityAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response)
                .Callback(() => _responded = true);
            
            _mockTurnContext.Setup(x => x.Responded).Returns(() => _responded);
            return this;
        }

        public ITurnContext Build() => _mockTurnContext.Object;

        public Mock<ITurnContext> GetMock() => _mockTurnContext;

        public static ITurnContext CreateMessageTurnContext(string message = "Test message")
        {
            return new TurnContextBuilder()
                .WithActivity(ActivityBuilder.CreateMessageActivity(message))
                .Build();
        }

        public static ITurnContext CreateConversationUpdateTurnContext(ChannelAccount? member = null)
        {
            member ??= new ChannelAccount { Id = "test-user", Name = "Test User" };
            return new TurnContextBuilder()
                .WithActivity(ActivityBuilder.CreateConversationUpdateActivity(member))
                .Build();
        }

        public static ITurnContext CreateRespondedTurnContext(string message = "Test message")
        {
            return new TurnContextBuilder()
                .WithActivity(ActivityBuilder.CreateMessageActivity(message))
                .WithResponded(true)
                .Build();
        }

        private void SetupDefaultMockBehavior()
        {
            _mockTurnContext.Setup(x => x.Activity).Returns(_activity);
            _mockTurnContext.Setup(x => x.Responded).Returns(() => _responded);
            _mockTurnContext
                .Setup(x => x.SendActivityAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceResponse { Id = "mock-response-id" })
                .Callback(() => _responded = true);
        }
    }
}
