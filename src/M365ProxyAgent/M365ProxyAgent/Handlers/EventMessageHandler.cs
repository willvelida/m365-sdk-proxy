using M365ProxyAgent.Exceptions;
using M365ProxyAgent.Interfaces;
using M365ProxyAgent.Services;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;

namespace M365ProxyAgent.Handlers
{
    /// <summary>
    /// Handler for event-type messages in conversations.
    /// Implements comprehensive error handling and structured logging.
    /// </summary>
    public class EventMessageHandler(
        IConversationService conversationService,
        ICorrelationService correlationService,
        ILogger<EventMessageHandler> logger) : IMessageHandler
    {
        private readonly IConversationService _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        private readonly ICorrelationService _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        private readonly ILogger<EventMessageHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public bool CanHandle(string activityType)
        {
            return activityType == ActivityTypes.Event;
        }

        public async Task HandleAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(turnContext);
            ArgumentNullException.ThrowIfNull(turnState);

            var correlationId = _correlationService.CorrelationId;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            _logger.LogInformation("Handling event message. CorrelationId: {CorrelationId}, EventName: {EventName}, UserId: {UserId}",
                correlationId, turnContext.Activity.Name, turnContext.Activity.From?.Id);

            try
            {
                var activityCount = 0;
                await foreach (Activity activity in _conversationService.ProcessMessageAsync((Activity)turnContext.Activity, cancellationToken))
                {
                    if (activity is not null)
                    {
                        await turnContext.SendActivityAsync(activity, cancellationToken);
                        activityCount++;

                        _logger.LogDebug("Sent response activity. CorrelationId: {CorrelationId}, ActivityType: {ActivityType}, ActivityId: {ActivityId}",
                            correlationId, activity.Type, activity.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Received null activity from conversation service. CorrelationId: {CorrelationId}", correlationId);
                    }
                }

                stopwatch.Stop();
                _logger.LogInformation("Event message handled successfully. CorrelationId: {CorrelationId}, ResponsesSent: {ResponsesSent}, Duration: {Duration}ms",
                    correlationId, activityCount, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex) when (ex is not ProxyAgentException)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to handle event message. CorrelationId: {CorrelationId}, EventName: {EventName}, Duration: {Duration}ms",
                    correlationId, turnContext.Activity.Name, stopwatch.ElapsedMilliseconds);

                throw new CopilotClientException(
                    "Failed to process event message",
                    "ProcessEvent",
                    null,
                    correlationId,
                    ex);
            }
        }
    }
}
