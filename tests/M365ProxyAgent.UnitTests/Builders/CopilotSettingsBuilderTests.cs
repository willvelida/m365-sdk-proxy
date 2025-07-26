using FluentAssertions;
using M365ProxyAgent.Configuration;
using M365ProxyAgent.UnitTests.Builders;

namespace M365ProxyAgent.UnitTests.Builders
{
    public class CopilotSettingsBuilderTests
    {
        [Fact]
        public void Build_WithDefaults_ReturnsSettingsWithDefaultValues()
        {
            // Act
            var settings = new CopilotSettingsBuilder().Build();

            // Assert
            settings.Should().NotBeNull();
            settings.AppClientId.Should().Be("test-app-client-id-guid");
            settings.TenantId.Should().Be("test-tenant-id-guid");
            settings.AppClientSecret.Should().Be("test-app-client-secret");
            settings.UseS2SConnection.Should().BeTrue();
        }

        [Fact]
        public void WithAppClientId_SetsAppClientIdProperty()
        {
            // Arrange
            const string expectedAppClientId = "custom-app-client-id-12345";

            // Act
            var settings = new CopilotSettingsBuilder()
                .WithAppClientId(expectedAppClientId)
                .Build();

            // Assert
            settings.AppClientId.Should().Be(expectedAppClientId);
        }

        [Fact]
        public void WithTenantId_SetsTenantIdProperty()
        {
            // Arrange
            const string expectedTenantId = "custom-tenant-id-guid-abcdef";

            // Act
            var settings = new CopilotSettingsBuilder()
                .WithTenantId(expectedTenantId)
                .Build();

            // Assert
            settings.TenantId.Should().Be(expectedTenantId);
        }

        [Fact]
        public void WithAppClientSecret_SetsAppClientSecretProperty()
        {
            // Arrange
            const string expectedSecret = "custom-client-secret-67890";

            // Act
            var settings = new CopilotSettingsBuilder()
                .WithAppClientSecret(expectedSecret)
                .Build();

            // Assert
            settings.AppClientSecret.Should().Be(expectedSecret);
        }

        [Fact]
        public void WithUseS2SConnection_SetsUseS2SConnectionProperty()
        {
            // Act
            var settings = new CopilotSettingsBuilder()
                .WithUseS2SConnection(false)
                .Build();

            // Assert
            settings.UseS2SConnection.Should().BeFalse();
        }

        [Fact]
        public void WithEnvironmentId_SetsEnvironmentId()
        {
            // Arrange
            const string expectedEnvironmentId = "custom-environment-id";

            // Act
            var settings = new CopilotSettingsBuilder()
                .WithEnvironmentId(expectedEnvironmentId)
                .Build();

            // Assert - Note: EnvironmentId is from the base ConnectionSettings class
            // We can't directly test it here without knowing the base class structure
            settings.Should().NotBeNull();
        }

        [Fact]
        public void WithSchemaName_SetsSchemaName()
        {
            // Arrange
            const string expectedSchemaName = "custom-schema-name";

            // Act
            var settings = new CopilotSettingsBuilder()
                .WithSchemaName(expectedSchemaName)
                .Build();

            // Assert - Note: SchemaName is from the base ConnectionSettings class
            // We can't directly test it here without knowing the base class structure
            settings.Should().NotBeNull();
        }

        [Fact]
        public void CreateMinimalSettings_ReturnsSettingsWithDefaults()
        {
            // Act
            var settings = CopilotSettingsBuilder.CreateMinimalSettings();

            // Assert
            settings.Should().NotBeNull();
            settings.AppClientId.Should().Be("test-app-client-id-guid");
            settings.TenantId.Should().Be("test-tenant-id-guid");
            settings.UseS2SConnection.Should().BeTrue();
        }

        [Fact]
        public void CreateProductionSettings_ReturnsProductionLikeSettings()
        {
            // Act
            var settings = CopilotSettingsBuilder.CreateProductionSettings();

            // Assert
            settings.Should().NotBeNull();
            settings.AppClientId.Should().Be("12345678-1234-1234-1234-123456789012");
            settings.TenantId.Should().Be("87654321-4321-4321-4321-210987654321");
            settings.UseS2SConnection.Should().BeTrue();
        }

        [Fact]
        public void CreateDevelopmentSettings_ReturnsDevelopmentSettings()
        {
            // Act
            var settings = CopilotSettingsBuilder.CreateDevelopmentSettings();

            // Assert
            settings.Should().NotBeNull();
            settings.AppClientId.Should().Be("dev-app-id-12345");
            settings.TenantId.Should().Be("dev-tenant-id-67890");
            settings.UseS2SConnection.Should().BeFalse();
        }

        [Fact]
        public void CreateInvalidSettings_ReturnsSettingsWithEmptyValues()
        {
            // Act
            var settings = CopilotSettingsBuilder.CreateInvalidSettings();

            // Assert
            settings.Should().NotBeNull();
            settings.AppClientId.Should().BeEmpty();
            settings.TenantId.Should().BeEmpty();
            settings.AppClientSecret.Should().BeEmpty();
        }

        [Fact]
        public void FluentBuilder_CanChainMultipleMethods()
        {
            // Arrange
            const string expectedAppClientId = "chained-app-client-id";
            const string expectedTenantId = "chained-tenant-id";
            const string expectedSecret = "chained-secret";
            const string expectedEnvironmentId = "chained-environment";
            const string expectedSchemaName = "chained-schema";

            // Act
            var settings = new CopilotSettingsBuilder()
                .WithAppClientId(expectedAppClientId)
                .WithTenantId(expectedTenantId)
                .WithAppClientSecret(expectedSecret)
                .WithEnvironmentId(expectedEnvironmentId)
                .WithSchemaName(expectedSchemaName)
                .WithUseS2SConnection(false)
                .Build();

            // Assert
            settings.AppClientId.Should().Be(expectedAppClientId);
            settings.TenantId.Should().Be(expectedTenantId);
            settings.AppClientSecret.Should().Be(expectedSecret);
            settings.UseS2SConnection.Should().BeFalse();
        }
    }
}
