using FluentValidation;
using M365ProxyAgent.Exceptions;
using M365ProxyAgent.Interfaces;

namespace M365ProxyAgent.Services
{
    /// <summary>
    /// Implementation of configuration validation service using FluentValidation.
    /// </summary>
    public class ConfigurationValidationService(
        IServiceProvider serviceProvider,
        ILogger<ConfigurationValidationService> logger) : IConfigurationValidationService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        private readonly ILogger<ConfigurationValidationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <inheritdoc />
        public void ValidateConfiguration<T>(T configuration) where T : class
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var result = ValidateConfigurationWithResult(configuration);

            if (!result.IsValid)
            {
                var errorMessage = $"Configuration validation failed for {typeof(T).Name}";
                _logger.LogError("Configuration validation failed for {ConfigurationType}: {Errors}",
                    typeof(T).Name, string.Join("; ", result.Errors));

                throw new ConfigurationException(
                    errorMessage,
                    typeof(T).Name,
                    result.Errors);
            }

            _logger.LogInformation("Configuration validation succeeded for {ConfigurationType}",
                typeof(T).Name);
        }

        /// <inheritdoc />
        public ConfigurationValidationResult ValidateConfigurationWithResult<T>(T configuration) where T : class
        {
            ArgumentNullException.ThrowIfNull(configuration);

            try
            {
                var validator = _serviceProvider.GetService<IValidator<T>>();

                if (validator == null)
                {
                    _logger.LogWarning("No validator found for configuration type {ConfigurationType}. Skipping validation.",
                        typeof(T).Name);
                    return new ConfigurationValidationResult(true, []);
                }

                var validationResult = validator.Validate(configuration);

                var errors = validationResult.Errors
                    .Select(error => $"{error.PropertyName}: {error.ErrorMessage}")
                    .ToArray();

                return new ConfigurationValidationResult(validationResult.IsValid, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during configuration validation for {ConfigurationType}",
                    typeof(T).Name);

                return new ConfigurationValidationResult(false,
                    [$"Validation error: {ex.Message}"]);
            }
        }
    }
}
