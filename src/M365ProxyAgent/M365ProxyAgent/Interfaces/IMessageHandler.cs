using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;

namespace M365ProxyAgent.Interfaces
{
    /// <summary>
    /// Interface for handling different types of messages in the proxy agent.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Handles the specified message asynchronously.
        /// </summary>
        /// <param name="turnContext">The turn context containing the message.</param>
        /// <param name="turnState">The current turn state.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken);

        /// <summary>
        /// Determines if this handler can process the given message type.
        /// </summary>
        /// <param name="activityType">The type of activity to check.</param>
        /// <returns>True if this handler can process the activity type, false otherwise.</returns>
        bool CanHandle(string activityType);
    }
}
