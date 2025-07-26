using M365ProxyAgent.Configuration;
using Microsoft.Extensions.Configuration;

namespace M365ProxyAgent.Tests.Builders
{
    public class CopilotSettingsBuilder
    {
        private string _appClientId = "test-app-client-id-guid";
        private string _tenantId = "test-tenant-id-guid";
        private string _appClientSecret = "test-app-client-secret";
        private string _environmentId = "test-environment-id";
        private string _schemaName = "test-schema-name";
        private bool _useS2SConnection = true;

        public CopilotSettingsBuilder WithAppClientId(string appClientId)
        {
            _appClientId = appClientId;
            return this;
        }

        public CopilotSettingsBuilder WithTenantId(string tenantId)
        {
            _tenantId = tenantId;
            return this;
        }

        public CopilotSettingsBuilder WithAppClientSecret(string appClientSecret)
        {
            _appClientSecret = appClientSecret;
            return this;
        }

        public CopilotSettingsBuilder WithEnvironmentId(string environmentId)
        {
            _environmentId = environmentId;
            return this;
        }

        public CopilotSettingsBuilder WithSchemaName(string schemaName)
        {
            _schemaName = schemaName;
            return this;
        }

        public CopilotSettingsBuilder WithUseS2SConnection(bool useS2SConnection)
        {
            _useS2SConnection = useS2SConnection;
            return this;
        }

        public CopilotStudioClientSettings Build()
        {
            var configData = new Dictionary<string, string?>
            {
                ["CopilotStudio:AppClientId"] = _appClientId,
                ["CopilotStudio:TenantId"] = _tenantId,
                ["CopilotStudio:AppClientSecret"] = _appClientSecret,
                ["CopilotStudio:EnvironmentId"] = _environmentId,
                ["CopilotStudio:SchemaName"] = _schemaName,
                ["CopilotStudio:UseS2SConnection"] = _useS2SConnection.ToString()
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            return new CopilotStudioClientSettings(configuration.GetSection("CopilotStudio"));
        }

        public static CopilotStudioClientSettings CreateMinimalSettings()
        {
            return new CopilotSettingsBuilder().Build();
        }

        public static CopilotStudioClientSettings CreateProductionSettings()
        {
            return new CopilotSettingsBuilder()
                .WithAppClientId("12345678-1234-1234-1234-123456789012")
                .WithTenantId("87654321-4321-4321-4321-210987654321")
                .WithEnvironmentId("prod-environment-id")
                .WithSchemaName("production-schema")
                .WithUseS2SConnection(true)
                .Build();
        }

        public static CopilotStudioClientSettings CreateDevelopmentSettings()
        {
            return new CopilotSettingsBuilder()
                .WithAppClientId("dev-app-id-12345")
                .WithTenantId("dev-tenant-id-67890")
                .WithEnvironmentId("dev-environment-id")
                .WithSchemaName("development-schema")
                .WithUseS2SConnection(false)
                .Build();
        }

        public static CopilotStudioClientSettings CreateInvalidSettings()
        {
            return new CopilotSettingsBuilder()
                .WithAppClientId("")
                .WithTenantId("")
                .WithAppClientSecret("")
                .WithEnvironmentId("")
                .WithSchemaName("")
                .Build();
        }
    }
}
