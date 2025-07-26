using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using M365ProxyAgent.Exceptions;
using M365ProxyAgent.Interfaces;
using M365ProxyAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace M365ProxyAgent.UnitTests.Services
{
    public class ConfigurationValidationServiceTests
    {
        private const string ValidName = "Valid";
        private const int ValidValue = 42;
        private const string TestName = "Test";
        private const int TestValue = 100;
        private const string EmptyName = "";
        private const int InvalidValue = -1;
        private const string NameRequiredMessage = "Name is required";
        private const string ValuePositiveMessage = "Value must be positive";
        private const string ValidationSucceededMessage = "Configuration validation succeeded";
        private const string ValidationFailedMessage = "Configuration validation failed for TestConfiguration";
        private const string NoValidatorMessage = "No validator found for configuration type TestConfiguration";
        private const string UnexpectedErrorMessage = "Unexpected error during configuration validation";
        private const string ValidatorInternalError = "Validator internal error";
        private const string ValidationErrorPrefix = "Validation error: ";
        
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<ILogger<ConfigurationValidationService>> _mockLogger;
        private readonly Mock<IValidator<TestConfiguration>> _mockValidator;
        private readonly ConfigurationValidationService _service;

        public ConfigurationValidationServiceTests()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockLogger = new Mock<ILogger<ConfigurationValidationService>>();
            _mockValidator = new Mock<IValidator<TestConfiguration>>();
            _service = new ConfigurationValidationService(_mockServiceProvider.Object, _mockLogger.Object);
        }

        [Fact]
        public void ValidateConfiguration_WithValidConfiguration_CompletesSuccessfully()
        {
            // Arrange
            var testConfiguration = CreateValidConfiguration();
            var validationResult = new ValidationResult(); // Empty result = valid
            
            SetupValidatorMock(testConfiguration, validationResult);

            // Act
            var act = () => _service.ValidateConfiguration(testConfiguration);

            // Assert
            act.Should().NotThrow();
            VerifyValidatorCalled(testConfiguration);
            VerifyLoggerCalled(LogLevel.Information, ValidationSucceededMessage);
        }

        [Fact]
        public void ValidateConfiguration_WithInvalidConfiguration_ThrowsConfigurationException()
        {
            // Arrange
            var testConfiguration = CreateInvalidConfiguration();
            var validationResult = CreateInvalidValidationResult();
            
            SetupValidatorMock(testConfiguration, validationResult);

            // Act
            var act = () => _service.ValidateConfiguration(testConfiguration);

            // Assert
            act.Should().Throw<ConfigurationException>()
                .WithMessage(ValidationFailedMessage)
                .And.ConfigurationSection.Should().Be("TestConfiguration");
            
            VerifyValidatorCalled(testConfiguration);
            VerifyLoggerCalled(LogLevel.Error, ValidationFailedMessage);
        }

        [Fact]
        public void ValidateConfiguration_WithNullConfiguration_ThrowsArgumentNullException()
        {
            // Act
            var act = () => _service.ValidateConfiguration<TestConfiguration>(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("configuration");
        }

        [Fact]
        public void ValidateConfiguration_WithNoValidator_CompletesSuccessfully()
        {
            // Arrange
            var testConfiguration = CreateTestConfiguration();
            SetupNoValidator();

            // Act
            var act = () => _service.ValidateConfiguration(testConfiguration);

            // Assert
            act.Should().NotThrow();
            VerifyLoggerCalled(LogLevel.Warning, NoValidatorMessage);
        }

        [Fact]
        public void ValidateConfigurationWithResult_WithValidConfiguration_ReturnsValidResult()
        {
            // Arrange
            var testConfiguration = CreateValidConfiguration();
            var validationResult = new ValidationResult(); // Empty result = valid
            
            SetupValidatorMock(testConfiguration, validationResult);

            // Act
            var result = _service.ValidateConfigurationWithResult(testConfiguration);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            VerifyValidatorCalled(testConfiguration);
        }

        [Fact]
        public void ValidateConfigurationWithResult_WithInvalidConfiguration_ReturnsInvalidResult()
        {
            // Arrange
            var testConfiguration = CreateInvalidConfiguration();
            var validationResult = CreateInvalidValidationResult();
            
            SetupValidatorMock(testConfiguration, validationResult);

            // Act
            var result = _service.ValidateConfigurationWithResult(testConfiguration);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().Contain("Name: " + NameRequiredMessage);
            result.Errors.Should().Contain("Value: " + ValuePositiveMessage);
            VerifyValidatorCalled(testConfiguration);
        }

        [Fact]
        public void ValidateConfigurationWithResult_WithNullConfiguration_ThrowsArgumentNullException()
        {
            // Act
            var act = () => _service.ValidateConfigurationWithResult<TestConfiguration>(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("configuration");
        }

        [Fact]
        public void ValidateConfigurationWithResult_WithNoValidator_ReturnsValidResult()
        {
            // Arrange
            var testConfiguration = CreateTestConfiguration();
            SetupNoValidator();

            // Act
            var result = _service.ValidateConfigurationWithResult(testConfiguration);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            VerifyLoggerCalled(LogLevel.Warning, NoValidatorMessage);
        }

        [Fact]
        public void ValidateConfigurationWithResult_WithValidatorException_ReturnsInvalidResult()
        {
            // Arrange
            var testConfiguration = CreateTestConfiguration();
            var validatorException = new InvalidOperationException(ValidatorInternalError);
            
            SetupValidatorException(testConfiguration, validatorException);

            // Act
            var result = _service.ValidateConfigurationWithResult(testConfiguration);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain(ValidationErrorPrefix + ValidatorInternalError);
            VerifyValidatorCalled(testConfiguration);
            VerifyLoggerCalled(LogLevel.Error, UnexpectedErrorMessage);
        }

        private static TestConfiguration CreateValidConfiguration()
        {
            return new TestConfiguration { Name = ValidName, Value = ValidValue };
        }

        private static TestConfiguration CreateInvalidConfiguration()
        {
            return new TestConfiguration { Name = EmptyName, Value = InvalidValue };
        }

        private static TestConfiguration CreateTestConfiguration()
        {
            return new TestConfiguration { Name = TestName, Value = TestValue };
        }

        private static ValidationResult CreateInvalidValidationResult()
        {
            var validationFailures = new[]
            {
                new ValidationFailure("Name", NameRequiredMessage),
                new ValidationFailure("Value", ValuePositiveMessage)
            };
            return new ValidationResult(validationFailures);
        }

        private void SetupValidatorMock(TestConfiguration configuration, ValidationResult result)
        {
            _mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IValidator<TestConfiguration>)))
                .Returns(_mockValidator.Object);
                
            _mockValidator
                .Setup(v => v.Validate(configuration))
                .Returns(result);
        }

        private void SetupValidatorException(TestConfiguration configuration, Exception exception)
        {
            _mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IValidator<TestConfiguration>)))
                .Returns(_mockValidator.Object);
                
            _mockValidator
                .Setup(v => v.Validate(configuration))
                .Throws(exception);
        }

        private void SetupNoValidator()
        {
            _mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IValidator<TestConfiguration>)))
                .Returns((object?)null);
        }

        private void VerifyValidatorCalled(TestConfiguration configuration)
        {
            _mockValidator.Verify(v => v.Validate(configuration), Times.Once);
        }

        private void VerifyLoggerCalled(LogLevel logLevel, string expectedMessage)
        {
            _mockLogger.Verify(
                x => x.Log(
                    logLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        public class TestConfiguration
        {
            public string Name { get; set; } = string.Empty;
            public int Value { get; set; }
        }
    }
}
