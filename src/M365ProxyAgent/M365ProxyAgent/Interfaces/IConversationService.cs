using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;

namespace M365ProxyAgent.Interfaces
{
    /// <summary>
    /// Interface for managing conversation lifecycle and operations.
    /// </summary>
    public interface IConversationService
    {
        /// <summary>
        /// Starts a new conversation and returns the initial activities.
        /// </summary>
        /// <param name="turnContext">The turn context for the conversation.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>An async enumerable of activities to send to the user.</returns>
        IAsyncEnumerable<Activity> StartConversationAsync(ITurnContext turnContext, CancellationToken cancellationToken);

        /// <summary>
        /// Processes a user message and returns the response activities.
        /// </summary>
        /// <param name="userActivity">The user's activity/message.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>An async enumerable of response activities.</returns>
        IAsyncEnumerable<Activity> ProcessMessageAsync(Activity userActivity, CancellationToken cancellationToken);
    }
}
