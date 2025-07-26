namespace M365ProxyAgent.Exceptions
{
    /// <summary>
    /// Exception thrown when configuration validation fails or configuration is invalid.
    /// </summary>
    public class ConfigurationException : ProxyAgentException
    {
        /// <summary>
        /// Gets the name of the configuration section that caused the error.
        /// </summary>
        public string? ConfigurationSection { get; }

        /// <summary>
        /// Gets the name of the specific configuration property that is invalid.
        /// </summary>
        public string? PropertyName { get; }

        /// <summary>
        /// Initializes a new instance of the ConfigurationException class.
        /// </summary>
        public ConfigurationException()
            : base("Configuration error occurred")
        {
        }

        /// <summary>
        /// Initializes a new instance of the ConfigurationException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the configuration error.</param>
        public ConfigurationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ConfigurationException class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the configuration error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ConfigurationException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ConfigurationException class with full context information.
        /// </summary>
        /// <param name="message">The message that describes the configuration error.</param>
        /// <param name="configurationSection">The configuration section that caused the error.</param>
        /// <param name="propertyName">The specific property that is invalid.</param>
        /// <param name="correlationId">The correlation ID for tracking the operation.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ConfigurationException(string message, string? configurationSection, string? propertyName, string? correlationId = null, Exception? innerException = null)
            : base(message, correlationId, CreateContext(configurationSection, propertyName), innerException)
        {
            ConfigurationSection = configurationSection;
            PropertyName = propertyName;
        }

        /// <summary>
        /// Initializes a new instance of the ConfigurationException class with validation errors.
        /// </summary>
        /// <param name="message">The message that describes the configuration error.</param>
        /// <param name="configurationSection">The configuration section that caused the error.</param>
        /// <param name="validationErrors">Collection of validation error messages.</param>
        /// <param name="correlationId">The correlation ID for tracking the operation.</param>
        public ConfigurationException(string message, string configurationSection, string[] validationErrors, string? correlationId = null)
            : base(message, correlationId, CreateValidationContext(configurationSection, validationErrors), null)
        {
            ConfigurationSection = configurationSection;
            PropertyName = null;
        }

        private static Dictionary<string, object>? CreateContext(string? configurationSection, string? propertyName)
        {
            var context = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(configurationSection))
                context["ConfigurationSection"] = configurationSection;

            if (!string.IsNullOrEmpty(propertyName))
                context["PropertyName"] = propertyName;

            return context.Count > 0 ? context : null;
        }

        private static Dictionary<string, object> CreateValidationContext(string configurationSection, string[] validationErrors)
        {
            return new Dictionary<string, object>
            {
                { "ConfigurationSection", configurationSection },
                { "ValidationErrors", validationErrors },
                { "ErrorCount", validationErrors.Length }
            };
        }
    }
}
