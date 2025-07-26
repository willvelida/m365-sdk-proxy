namespace M365ProxyAgent.Interfaces
{
    /// <summary>
    /// Interface for managing correlation IDs for request tracking.
    /// </summary>
    public interface ICorrelationService
    {
        /// <summary>
        /// Gets the current correlation ID for the request.
        /// </summary>
        string CorrelationId { get; }

        /// <summary>
        /// Generates a new correlation ID.
        /// </summary>
        /// <returns>A new unique correlation ID.</returns>
        string GenerateCorrelationId();

        /// <summary>
        /// Sets the correlation ID for the current scope.
        /// </summary>
        /// <param name="correlationId">The correlation ID to set.</param>
        void SetCorrelationId(string correlationId);
    }
}
