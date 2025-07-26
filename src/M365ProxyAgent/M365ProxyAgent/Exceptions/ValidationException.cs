namespace M365ProxyAgent.Exceptions
{
    /// <summary>
    /// Exception thrown when input validation fails or validation rules are violated.
    /// Used for request validation, data validation, and business rule violations.
    /// </summary>
    public class ValidationException : ProxyAgentException
    {
        /// <summary>
        /// Gets the collection of validation errors.
        /// </summary>
        public string[] ValidationErrors { get; }

        /// <summary>
        /// Gets the name of the field or property that failed validation.
        /// </summary>
        public string? FieldName { get; }

        /// <summary>
        /// Gets the invalid value that caused the validation failure.
        /// </summary>
        public object? InvalidValue { get; }

        /// <summary>
        /// Initializes a new instance of the ValidationException class.
        /// </summary>
        public ValidationException()
            : base("Validation error occurred")
        {
            ValidationErrors = [];
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the validation error.</param>
        public ValidationException(string message)
            : base(message)
        {
            ValidationErrors = [];
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the validation error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ValidationException(string message, Exception? innerException)
            : base(message, innerException)
        {
            ValidationErrors = [];
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with validation errors.
        /// </summary>
        /// <param name="message">The message that describes the validation error.</param>
        /// <param name="validationErrors">Collection of validation error messages.</param>
        /// <param name="correlationId">The correlation ID for tracking the operation.</param>
        public ValidationException(string message, string[] validationErrors, string? correlationId = null)
            : base(message, correlationId, CreateValidationContext(validationErrors), null)
        {
            ValidationErrors = validationErrors ?? [];
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with field-specific validation error.
        /// </summary>
        /// <param name="message">The message that describes the validation error.</param>
        /// <param name="fieldName">The name of the field that failed validation.</param>
        /// <param name="invalidValue">The invalid value that caused the validation failure.</param>
        /// <param name="correlationId">The correlation ID for tracking the operation.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ValidationException(string message, string fieldName, object? invalidValue, string? correlationId = null, Exception? innerException = null)
            : base(message, correlationId, CreateFieldValidationContext(fieldName, invalidValue), innerException)
        {
            FieldName = fieldName;
            InvalidValue = invalidValue;
            ValidationErrors = [$"{fieldName}: {message}"];
        }

        /// <summary>
        /// Creates validation context with error details.
        /// </summary>
        /// <param name="validationErrors">The validation errors to include in context.</param>
        /// <returns>Dictionary containing validation context information.</returns>
        private static Dictionary<string, object> CreateValidationContext(string[] validationErrors)
        {
            return new Dictionary<string, object>
            {
                { "ValidationErrors", validationErrors },
                { "ErrorCount", validationErrors.Length },
                { "ErrorType", "MultipleValidationErrors" }
            };
        }

        /// <summary>
        /// Creates field-specific validation context.
        /// </summary>
        /// <param name="fieldName">The field name that failed validation.</param>
        /// <param name="invalidValue">The invalid value.</param>
        /// <returns>Dictionary containing field validation context information.</returns>
        private static Dictionary<string, object> CreateFieldValidationContext(string fieldName, object? invalidValue)
        {
            var context = new Dictionary<string, object>
            {
                { "FieldName", fieldName },
                { "ErrorType", "FieldValidationError" }
            };

            if (invalidValue != null)
            {
                context["InvalidValue"] = invalidValue.ToString() ?? "null";
                context["InvalidValueType"] = invalidValue.GetType().Name;
            }

            return context;
        }
    }
}
