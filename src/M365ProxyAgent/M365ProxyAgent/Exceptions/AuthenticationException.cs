namespace M365ProxyAgent.Exceptions
{
    /// <summary>
    /// Exception thrown when authentication operations fail.
    /// </summary>
    public class AuthenticationException : ProxyAgentException
    {
        /// <summary>
        /// Gets the authentication method that failed.
        /// </summary>
        public string? AuthenticationMethod { get; }

        /// <summary>
        /// Gets the tenant ID associated with the failed authentication.
        /// </summary>
        public string? TenantId { get; }

        /// <summary>
        /// Initializes a new instance of the AuthenticationException class.
        /// </summary>
        public AuthenticationException()
            : base("Authentication failed")
        {
        }

        /// <summary>
        /// Initializes a new instance of the AuthenticationException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the authentication error.</param>
        public AuthenticationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AuthenticationException class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the authentication error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public AuthenticationException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AuthenticationException class with full context information.
        /// </summary>
        /// <param name="message">The message that describes the authentication error.</param>
        /// <param name="authenticationMethod">The authentication method that failed.</param>
        /// <param name="tenantId">The tenant ID associated with the failed authentication.</param>
        /// <param name="correlationId">The correlation ID for tracking the operation.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public AuthenticationException(string message, string? authenticationMethod, string? tenantId, string? correlationId, Exception? innerException = null)
            : base(message, correlationId, CreateContext(authenticationMethod, tenantId), innerException)
        {
            AuthenticationMethod = authenticationMethod;
            TenantId = tenantId;
        }

        private static Dictionary<string, object>? CreateContext(string? authenticationMethod, string? tenantId)
        {
            var context = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(authenticationMethod))
                context["AuthenticationMethod"] = authenticationMethod;

            if (!string.IsNullOrEmpty(tenantId))
                context["TenantId"] = tenantId;

            return context.Count > 0 ? context : null;
        }
    }
}
