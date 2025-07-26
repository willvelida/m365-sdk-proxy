using M365ProxyAgent.Interfaces;

namespace M365ProxyAgent.Services
{
    /// <summary>
    /// Service for managing correlation IDs using AsyncLocal for thread-safe context.
    /// </summary>
    public class CorrelationService : ICorrelationService
    {
        private static readonly AsyncLocal<string?> _correlationId = new();

        /// <summary>
        /// Gets the current correlation ID, generating one if none exists.
        /// </summary>
        public string CorrelationId => _correlationId.Value ?? GenerateCorrelationId();

        /// <summary>
        /// Generates a new unique correlation ID.
        /// </summary>
        /// <returns>A new unique correlation ID based on GUID.</returns>
        public string GenerateCorrelationId()
        {
            var correlationId = Guid.NewGuid().ToString("D");
            _correlationId.Value = correlationId;
            return correlationId;
        }

        /// <summary>
        /// Sets the correlation ID for the current async context.
        /// </summary>
        /// <param name="correlationId">The correlation ID to set.</param>
        public void SetCorrelationId(string correlationId)
        {
            ArgumentNullException.ThrowIfNull(correlationId);
            _correlationId.Value = correlationId;
        }
    }
}
