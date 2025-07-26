using M365ProxyAgent.Exceptions;
using M365ProxyAgent.Interfaces;
using M365ProxyAgent.Services;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;

namespace M365ProxyAgent.Handlers
{
    /// <summary>
    /// Handler for welcome messages when new members are added to a conversation.
    /// Implements comprehensive error handling and structured logging.
    /// </summary>
    public class WelcomeMessageHandler(
        IConversationService conversationService,
        ICorrelationService correlationService,
        ILogger<WelcomeMessageHandler> logger) : IMessageHandler
    {
        private readonly IConversationService _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        private readonly ICorrelationService _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        private readonly ILogger<WelcomeMessageHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public bool CanHandle(string activityType)
        {
            return activityType == ActivityTypes.ConversationUpdate;
        }

        public async Task HandleAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(turnContext);
            ArgumentNullException.ThrowIfNull(turnState);

            var correlationId = _correlationService.CorrelationId;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            _logger.LogInformation("Handling welcome message for conversation update. CorrelationId: {CorrelationId}, ConversationId: {ConversationId}",
                correlationId, turnContext.Activity.Conversation?.Id);

            try
            {
                var membersProcessed = 0;
                foreach (ChannelAccount member in turnContext.Activity.MembersAdded ?? Enumerable.Empty<ChannelAccount>())
                {
                    if (member.Id != turnContext.Activity.Recipient?.Id)
                    {
                        _logger.LogDebug("Starting conversation for new member. CorrelationId: {CorrelationId}, MemberId: {MemberId}, MemberName: {MemberName}",
                            correlationId, member.Id, member.Name);

                        var activityCount = 0;
                        await foreach (Activity activity in _conversationService.StartConversationAsync(turnContext, cancellationToken))
                        {
                            if (activity is null)
                            {
                                _logger.LogWarning("Received null activity from conversation service. CorrelationId: {CorrelationId}, MemberId: {MemberId}",
                                    correlationId, member.Id);
                                continue;
                            }

                            await turnContext.SendActivityAsync(activity, cancellationToken);
                            activityCount++;

                            _logger.LogDebug("Sent welcome activity. CorrelationId: {CorrelationId}, ActivityType: {ActivityType}, ActivityId: {ActivityId}",
                                correlationId, activity.Type, activity.Id);
                        }

                        membersProcessed++;
                        _logger.LogInformation("Welcome conversation completed for member. CorrelationId: {CorrelationId}, MemberId: {MemberId}, ActivitiesSent: {ActivitiesSent}",
                            correlationId, member.Id, activityCount);
                    }
                }

                stopwatch.Stop();
                _logger.LogInformation("Welcome message handling completed. CorrelationId: {CorrelationId}, MembersProcessed: {MembersProcessed}, Duration: {Duration}ms",
                    correlationId, membersProcessed, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex) when (ex is not ProxyAgentException)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to handle welcome message. CorrelationId: {CorrelationId}, Duration: {Duration}ms",
                    correlationId, stopwatch.ElapsedMilliseconds);

                throw new CopilotClientException(
                    "Failed to process welcome message",
                    "WelcomeMessage",
                    null,
                    correlationId,
                    ex);
            }
        }
    }
}
