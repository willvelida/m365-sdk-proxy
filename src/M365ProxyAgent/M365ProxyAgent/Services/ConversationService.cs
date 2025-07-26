using M365ProxyAgent.Exceptions;
using M365ProxyAgent.Interfaces;
using Microsoft.Agents.Builder;
using Microsoft.Agents.CopilotStudio.Client;
using Microsoft.Agents.Core.Models;
using System.Runtime.CompilerServices;

namespace M365ProxyAgent.Services
{
    /// <summary>
    /// Service responsible for managing conversation lifecycle and communication with Copilot Studio.
    /// Implements comprehensive error handling, structured logging, and resilience patterns.
    /// </summary>
    public class ConversationService(
        CopilotClient copilotClient,
        ICorrelationService correlationService,
        IResilienceService resilienceService,
        ILogger<ConversationService> logger) : IConversationService
    {
        private readonly CopilotClient _copilotClient = copilotClient ?? throw new ArgumentNullException(nameof(copilotClient));
        private readonly ICorrelationService _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        private readonly IResilienceService _resilienceService = resilienceService ?? throw new ArgumentNullException(nameof(resilienceService));
        private readonly ILogger<ConversationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async IAsyncEnumerable<Activity> StartConversationAsync(ITurnContext turnContext, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(turnContext);

            var correlationId = _correlationService.CorrelationId;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            _logger.LogInformation("Starting new conversation. CorrelationId: {CorrelationId}, UserId: {UserId}",
                correlationId, turnContext.Activity.From?.Id);

            var activityCount = 0;
            var activitiesEnumerable = _resilienceService.CopilotClientPipeline.Execute(() =>
                _copilotClient.StartConversationAsync(emitStartConversationEvent: true, cancellationToken));

            await foreach (var activityInterface in activitiesEnumerable)
            {
                if (activityInterface is not Activity activity)
                {
                    _logger.LogWarning("Received null or invalid activity from CopilotClient.StartConversationAsync. CorrelationId: {CorrelationId}", correlationId);
                    continue;
                }

                activityCount++;
                _logger.LogDebug("Yielding conversation start activity. CorrelationId: {CorrelationId}, ActivityId: {ActivityId}, ActivityType: {ActivityType}",
                    correlationId, activity.Id, activity.Type);

                yield return activity;
            }

            stopwatch.Stop();
            _logger.LogInformation("Conversation started successfully. CorrelationId: {CorrelationId}, ActivityCount: {ActivityCount}, Duration: {Duration}ms",
                correlationId, activityCount, stopwatch.ElapsedMilliseconds);
        }

        public async IAsyncEnumerable<Activity> ProcessMessageAsync(Activity userActivity, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(userActivity);

            var correlationId = _correlationService.CorrelationId;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            _logger.LogInformation("Processing user message. CorrelationId: {CorrelationId}, MessageText: {MessageText}, UserId: {UserId}",
                correlationId, userActivity.Text, userActivity.From?.Id);

            var activityCount = 0;
            var activitiesEnumerable = _resilienceService.CopilotClientPipeline.Execute(() =>
                _copilotClient.AskQuestionAsync(userActivity, cancellationToken));

            await foreach (var activityInterface in activitiesEnumerable)
            {
                if (activityInterface is not Activity activity)
                {
                    _logger.LogWarning("Received null or invalid activity from CopilotClient.AskQuestionAsync. CorrelationId: {CorrelationId}", correlationId);
                    continue;
                }

                activityCount++;
                _logger.LogDebug("Yielding response activity. CorrelationId: {CorrelationId}, ActivityId: {ActivityId}, ActivityType: {ActivityType}",
                    correlationId, activity.Id, activity.Type);

                yield return activity;
            }

            stopwatch.Stop();
            _logger.LogInformation("Message processed successfully. CorrelationId: {CorrelationId}, ResponseCount: {ResponseCount}, Duration: {Duration}ms",
                correlationId, activityCount, stopwatch.ElapsedMilliseconds);
        }
    }
}
