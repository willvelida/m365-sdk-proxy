using M365ProxyAgent.Exceptions;

namespace M365ProxyAgent.Interfaces
{
    /// <summary>
    /// Service for validating configuration objects using FluentValidation.
    /// Provides centralized configuration validation with detailed error reporting.
    /// </summary>
    public interface IConfigurationValidationService
    {
        /// <summary>
        /// Validates a configuration object using its registered validator.
        /// </summary>
        /// <typeparam name="T">The type of configuration object to validate</typeparam>
        /// <param name="configuration">The configuration object to validate</param>
        /// <exception cref="ConfigurationException">Thrown when validation fails</exception>
        void ValidateConfiguration<T>(T configuration) where T : class;

        /// <summary>
        /// Validates a configuration object and returns validation results.
        /// </summary>
        /// <typeparam name="T">The type of configuration object to validate</typeparam>
        /// <param name="configuration">The configuration object to validate</param>
        /// <returns>Validation result with success status and error details</returns>
        ConfigurationValidationResult ValidateConfigurationWithResult<T>(T configuration) where T : class;
    }

    /// <summary>
    /// Result of configuration validation operation.
    /// </summary>
    /// <param name="IsValid">Indicates whether the configuration is valid</param>
    /// <param name="Errors">Collection of validation error messages</param>
    public record ConfigurationValidationResult(bool IsValid, string[] Errors);
}
