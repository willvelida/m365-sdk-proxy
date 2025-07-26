namespace M365ProxyAgent.Exceptions
{
    /// <summary>
    /// Base exception class for all M365 Proxy Agent domain-specific errors.
    /// Provides common functionality and context for all application exceptions.
    /// </summary>
    public abstract class ProxyAgentException : Exception
    {
        /// <summary>
        /// Gets the correlation ID associated with the operation that caused this exception.
        /// </summary>
        public string? CorrelationId { get; }

        /// <summary>
        /// Gets additional context information about the error.
        /// </summary>
        public Dictionary<string, object>? Context { get; }

        /// <summary>
        /// Initializes a new instance of the ProxyAgentException class.
        /// </summary>
        protected ProxyAgentException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ProxyAgentException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        protected ProxyAgentException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ProxyAgentException class with a specified error message and correlation ID.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="correlationId">The correlation ID for tracking the operation.</param>
        protected ProxyAgentException(string message, string? correlationId)
            : base(message)
        {
            CorrelationId = correlationId;
        }

        /// <summary>
        /// Initializes a new instance of the ProxyAgentException class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        protected ProxyAgentException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ProxyAgentException class with full context information.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="correlationId">The correlation ID for tracking the operation.</param>
        /// <param name="context">Additional context information about the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        protected ProxyAgentException(string message, string? correlationId, Dictionary<string, object>? context, Exception? innerException = null)
            : base(message, innerException)
        {
            CorrelationId = correlationId;
            Context = context;
        }
    }
}
