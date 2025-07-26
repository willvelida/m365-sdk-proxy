using FluentValidation;
using M365ProxyAgent.Configuration;

namespace M365ProxyAgent.Validation
{
    /// <summary>
    /// Validator for CopilotStudioClientSettings configuration section.
    /// Ensures all required settings are present and valid.
    /// </summary>
    public class CopilotStudioClientSettingsValidator : AbstractValidator<CopilotStudioClientSettings>
    {
        public CopilotStudioClientSettingsValidator()
        {
            // Tenant ID validation
            RuleFor(x => x.TenantId)
                .NotEmpty()
                .WithMessage("TenantId is required for authentication")
                .Matches(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")
                .WithMessage("TenantId must be a valid GUID format")
                .When(x => !string.IsNullOrEmpty(x.TenantId));

            // Application Client ID validation
            RuleFor(x => x.AppClientId)
                .NotEmpty()
                .WithMessage("AppClientId is required for authentication")
                .Matches(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")
                .WithMessage("AppClientId must be a valid GUID format")
                .When(x => !string.IsNullOrEmpty(x.AppClientId));

            // Application Client Secret validation
            RuleFor(x => x.AppClientSecret)
                .NotEmpty()
                .WithMessage("AppClientSecret is required for authentication")
                .MinimumLength(10)
                .WithMessage("AppClientSecret must be at least 10 characters long")
                .Must(NotContainWhitespace)
                .WithMessage("AppClientSecret cannot contain whitespace characters");

            // S2S Connection validation
            RuleFor(x => x.UseS2SConnection)
                .NotNull()
                .WithMessage("UseS2SConnection must be explicitly set to true or false");
        }

        /// <summary>
        /// Validates that the string does not contain whitespace characters.
        /// </summary>
        /// <param name="value">The string to validate</param>
        /// <returns>True if the string contains no whitespace, false otherwise</returns>
        private static bool NotContainWhitespace(string? value)
        {
            return value != null && !value.Any(char.IsWhiteSpace);
        }
    }
}
