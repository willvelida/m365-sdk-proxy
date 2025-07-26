namespace M365ProxyAgent.Exceptions
{
    /// <summary>
    /// Exception thrown when communication with Copilot Studio fails.
    /// </summary>
    public class CopilotClientException : ProxyAgentException
    {
        /// <summary>
        /// Gets the operation that failed when communicating with Copilot Studio.
        /// </summary>
        public string? Operation { get; }

        /// <summary>
        /// Gets the HTTP status code if the error was related to an HTTP request.
        /// </summary>
        public int? HttpStatusCode { get; }

        /// <summary>
        /// Initializes a new instance of the CopilotClientException class.
        /// </summary>
        public CopilotClientException()
            : base("Copilot Studio communication error occurred")
        {
        }

        /// <summary>
        /// Initializes a new instance of the CopilotClientException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the Copilot Studio communication error.</param>
        public CopilotClientException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CopilotClientException class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the Copilot Studio communication error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public CopilotClientException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CopilotClientException class with full context information.
        /// </summary>
        /// <param name="message">The message that describes the Copilot Studio communication error.</param>
        /// <param name="operation">The operation that failed.</param>
        /// <param name="httpStatusCode">The HTTP status code if applicable.</param>
        /// <param name="correlationId">The correlation ID for tracking the operation.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public CopilotClientException(string message, string? operation, int? httpStatusCode, string? correlationId, Exception? innerException = null)
            : base(message, correlationId, CreateContext(operation, httpStatusCode), innerException)
        {
            Operation = operation;
            HttpStatusCode = httpStatusCode;
        }

        private static Dictionary<string, object>? CreateContext(string? operation, int? httpStatusCode)
        {
            var context = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(operation))
                context["Operation"] = operation;

            if (httpStatusCode.HasValue)
                context["HttpStatusCode"] = httpStatusCode.Value;

            return context.Count > 0 ? context : null;
        }
    }
}
