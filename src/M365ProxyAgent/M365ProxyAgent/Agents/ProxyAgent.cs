using M365ProxyAgent.Handlers;
using M365ProxyAgent.Interfaces;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;

namespace M365ProxyAgent.Agents
{
    /// <summary>
    /// Main proxy agent that orchestrates message handling using SOLID principles.
    /// Follows Single Responsibility Principle by delegating specific tasks to specialized handlers.
    /// </summary>
    public class ProxyAgent : AgentApplication
    {
        private readonly IMessageHandlerFactory _messageHandlerFactory;
        private readonly ILogger<ProxyAgent> _logger;

        public ProxyAgent(
            AgentApplicationOptions options,
            IMessageHandlerFactory messageHandlerFactory,
            ILogger<ProxyAgent> logger) : base(options)
        {
            _messageHandlerFactory = messageHandlerFactory ?? throw new ArgumentNullException(nameof(messageHandlerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            OnConversationUpdate(ConversationUpdateEvents.MembersAdded, HandleConversationUpdateAsync);
            OnActivity(ActivityTypes.Message, HandleMessageAsync, rank: RouteRank.Last);
        }

        /// <summary>
        /// Handles conversation update events (e.g., members added) by delegating to appropriate handler.
        /// </summary>
        private async Task HandleConversationUpdateAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(turnContext);
            ArgumentNullException.ThrowIfNull(turnState);

            _logger.LogInformation("Handling conversation update event");

            var handler = _messageHandlerFactory.CreateHandler(ActivityTypes.ConversationUpdate);
            if (handler is not null)
            {
                await handler.HandleAsync(turnContext, turnState, cancellationToken);
            }
            else
            {
                _logger.LogWarning("No handler found for conversation update activity");
            }
        }

        /// <summary>
        /// Handles regular message activities by delegating to appropriate handler.
        /// </summary>
        private async Task HandleMessageAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(turnContext);
            ArgumentNullException.ThrowIfNull(turnState);

            _logger.LogInformation("Handling message activity");

            var handler = _messageHandlerFactory.CreateHandler(ActivityTypes.Message);
            if (handler is not null)
            {
                await handler.HandleAsync(turnContext, turnState, cancellationToken);
            }
            else
            {
                _logger.LogWarning("No handler found for message activity");
            }
        }
    }
}
