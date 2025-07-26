namespace M365ProxyAgent.Interfaces
{
    /// <summary>
    /// Factory for creating appropriate message handlers based on activity type.
    /// </summary>
    public interface IMessageHandlerFactory
    {
        /// <summary>
        /// Creates a message handler for the specified activity type.
        /// </summary>
        /// <param name="activityType">The type of activity to handle.</param>
        /// <returns>A message handler that can process the activity type.</returns>
        IMessageHandler? CreateHandler(string activityType);
    }
}
