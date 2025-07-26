using FluentValidation;
using M365ProxyAgent.Configuration;
using M365ProxyAgent.Interfaces;
using M365ProxyAgent.Services;
using M365ProxyAgent.Validation;

namespace M365ProxyAgent.Extensions
{
    /// <summary>
    /// Extension methods for adding configuration validation to the service collection.
    /// </summary>
    public static class ConfigurationValidationExtensions
    {
        /// <summary>
        /// Adds FluentValidation services and configuration validators to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddConfigurationValidation(this IServiceCollection services)
        {
            // Register FluentValidation validators as transient instead of scoped
            services.AddValidatorsFromAssemblyContaining<CopilotStudioClientSettingsValidator>(
                ServiceLifetime.Transient);

            // Add configuration validation service
            services.AddSingleton<IConfigurationValidationService, ConfigurationValidationService>();

            return services;
        }

        /// <summary>
        /// Validates critical configuration objects during application startup.
        /// This method should be called after the service provider is built but before the application starts.
        /// </summary>
        /// <param name="serviceProvider">The service provider containing validation services</param>
        /// <param name="configuration">The application configuration</param>
        /// <exception cref="InvalidOperationException">Thrown when critical configuration validation fails</exception>
        public static void ValidateStartupConfiguration(this IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var validationService = serviceProvider.GetRequiredService<IConfigurationValidationService>();
            var logger = serviceProvider.GetRequiredService<ILogger<IServiceProvider>>();

            logger.LogInformation("Starting configuration validation...");

            try
            {
                // Validate CopilotStudioClientSettings
                var copilotSettingsSection = configuration.GetSection("CopilotStudioClientSettings");
                if (copilotSettingsSection.Exists())
                {
                    var copilotSettings = new CopilotStudioClientSettings(copilotSettingsSection);
                    validationService.ValidateConfiguration(copilotSettings);
                    logger.LogInformation("CopilotStudioClientSettings validation completed successfully");
                }
                else
                {
                    logger.LogWarning("CopilotStudioClientSettings section not found in configuration");
                }

                // Validate other critical configuration sections here as needed
                // Example: ValidateAgentApplicationOptions, ValidateTokenValidation, etc.

                logger.LogInformation("All configuration validation completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Configuration validation failed during startup");
                throw new InvalidOperationException(
                    $"Application startup failed due to configuration validation errors: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates a specific configuration section from IConfiguration.
        /// </summary>
        /// <typeparam name="T">The type of configuration object</typeparam>
        /// <param name="serviceProvider">The service provider</param>
        /// <param name="configuration">The application configuration</param>
        /// <param name="sectionName">The name of the configuration section</param>
        /// <param name="configurationFactory">Factory method to create the configuration object from IConfigurationSection</param>
        /// <returns>The validated configuration object</returns>
        public static T ValidateConfigurationSection<T>(
            this IServiceProvider serviceProvider,
            IConfiguration configuration,
            string sectionName,
            Func<IConfigurationSection, T> configurationFactory) where T : class
        {
            var validationService = serviceProvider.GetRequiredService<IConfigurationValidationService>();
            var section = configuration.GetSection(sectionName);

            if (!section.Exists())
            {
                throw new InvalidOperationException($"Required configuration section '{sectionName}' not found");
            }

            var configObj = configurationFactory(section);
            validationService.ValidateConfiguration(configObj);

            return configObj;
        }
    }
}
