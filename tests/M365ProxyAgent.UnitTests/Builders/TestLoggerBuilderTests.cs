using FluentAssertions;
using M365ProxyAgent.UnitTests.Builders;
using Microsoft.Extensions.Logging;
using Moq;

namespace M365ProxyAgent.UnitTests.Builders
{
    public class TestLoggerBuilderTests
    {
        [Fact]
        public void Build_WithDefaults_ReturnsLoggerWithDefaultBehavior()
        {
            // Act
            var logger = new TestLoggerBuilder<TestLoggerBuilderTests>().Build();

            // Assert
            logger.Should().NotBeNull();
            logger.IsEnabled(LogLevel.Trace).Should().BeTrue();
            logger.IsEnabled(LogLevel.Debug).Should().BeTrue();
            logger.IsEnabled(LogLevel.Information).Should().BeTrue();
            logger.IsEnabled(LogLevel.Warning).Should().BeTrue();
            logger.IsEnabled(LogLevel.Error).Should().BeTrue();
            logger.IsEnabled(LogLevel.Critical).Should().BeTrue();
        }

        [Fact]
        public void WithMinLogLevel_Information_EnablesAppropriateLogLevels()
        {
            // Act
            var logger = new TestLoggerBuilder<TestLoggerBuilderTests>()
                .WithMinLogLevel(LogLevel.Information)
                .Build();

            // Assert
            logger.IsEnabled(LogLevel.Trace).Should().BeFalse();
            logger.IsEnabled(LogLevel.Debug).Should().BeFalse();
            logger.IsEnabled(LogLevel.Information).Should().BeTrue();
            logger.IsEnabled(LogLevel.Warning).Should().BeTrue();
            logger.IsEnabled(LogLevel.Error).Should().BeTrue();
            logger.IsEnabled(LogLevel.Critical).Should().BeTrue();
        }

        [Fact]
        public void WithMinLogLevel_Error_EnablesOnlyErrorAndCritical()
        {
            // Act
            var logger = new TestLoggerBuilder<TestLoggerBuilderTests>()
                .WithMinLogLevel(LogLevel.Error)
                .Build();

            // Assert
            logger.IsEnabled(LogLevel.Trace).Should().BeFalse();
            logger.IsEnabled(LogLevel.Debug).Should().BeFalse();
            logger.IsEnabled(LogLevel.Information).Should().BeFalse();
            logger.IsEnabled(LogLevel.Warning).Should().BeFalse();
            logger.IsEnabled(LogLevel.Error).Should().BeTrue();
            logger.IsEnabled(LogLevel.Critical).Should().BeTrue();
        }

        [Fact]
        public void WithMinLogLevel_None_DisablesAllLogLevels()
        {
            // Act
            var logger = new TestLoggerBuilder<TestLoggerBuilderTests>()
                .WithMinLogLevel(LogLevel.None)
                .Build();

            // Assert
            logger.IsEnabled(LogLevel.Trace).Should().BeFalse();
            logger.IsEnabled(LogLevel.Debug).Should().BeFalse();
            logger.IsEnabled(LogLevel.Information).Should().BeFalse();
            logger.IsEnabled(LogLevel.Warning).Should().BeFalse();
            logger.IsEnabled(LogLevel.Error).Should().BeFalse();
            logger.IsEnabled(LogLevel.Critical).Should().BeFalse();
        }

        [Fact]
        public void WithExceptionOnLog_ThrowsExceptionForSpecificLogLevel()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Test exception");
            var logger = new TestLoggerBuilder<TestLoggerBuilderTests>()
                .WithExceptionOnLog(LogLevel.Error, expectedException)
                .Build();

            // Act & Assert
            var act = () => logger.LogError("This should throw");
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Test exception");
        }

        [Fact]
        public void GetMock_ReturnsUnderlyingMock()
        {
            // Arrange
            var builder = new TestLoggerBuilder<TestLoggerBuilderTests>();

            // Act
            var mock = builder.GetMock();

            // Assert
            mock.Should().NotBeNull();
            mock.Should().BeOfType<Mock<ILogger<TestLoggerBuilderTests>>>();
        }

        [Fact]
        public void CreateLogger_ReturnsBasicLogger()
        {
            // Act
            var logger = TestLoggerBuilder<TestLoggerBuilderTests>.CreateLogger();

            // Assert
            logger.Should().NotBeNull();
            logger.IsEnabled(LogLevel.Information).Should().BeTrue();
        }

        [Fact]
        public void CreateCapturingLogger_ReturnsCapturingLogger()
        {
            // Act
            var logger = TestLoggerBuilder<TestLoggerBuilderTests>.CreateCapturingLogger();

            // Assert
            logger.Should().NotBeNull();
            // Test that it doesn't throw when logging
            var act = () => logger.LogInformation("Test message");
            act.Should().NotThrow();
        }

        [Fact]
        public void CreateLoggerWithLevel_Warning_ReturnsLoggerWithCorrectLevel()
        {
            // Act
            var logger = TestLoggerBuilder<TestLoggerBuilderTests>.CreateLoggerWithLevel(LogLevel.Warning);

            // Assert
            logger.IsEnabled(LogLevel.Debug).Should().BeFalse();
            logger.IsEnabled(LogLevel.Information).Should().BeFalse();
            logger.IsEnabled(LogLevel.Warning).Should().BeTrue();
            logger.IsEnabled(LogLevel.Error).Should().BeTrue();
        }

        [Fact]
        public void CreateSilentLogger_ReturnsLoggerThatLogsNothing()
        {
            // Act
            var logger = TestLoggerBuilder<TestLoggerBuilderTests>.CreateSilentLogger();

            // Assert
            logger.IsEnabled(LogLevel.Critical).Should().BeFalse();
            logger.IsEnabled(LogLevel.Error).Should().BeFalse();
            logger.IsEnabled(LogLevel.Warning).Should().BeFalse();
            logger.IsEnabled(LogLevel.Information).Should().BeFalse();
            logger.IsEnabled(LogLevel.Debug).Should().BeFalse();
            logger.IsEnabled(LogLevel.Trace).Should().BeFalse();
        }

        [Fact]
        public void StaticFor_ReturnsBuilderForSpecifiedType()
        {
            // Act
            var builder = TestLoggerBuilder.For<TestLoggerBuilderTests>();

            // Assert
            builder.Should().NotBeNull();
            builder.Should().BeOfType<TestLoggerBuilder<TestLoggerBuilderTests>>();
        }

        [Fact]
        public void StaticCreateLogger_ReturnsLogger()
        {
            // Act
            var logger = TestLoggerBuilder.CreateLogger<TestLoggerBuilderTests>();

            // Assert
            logger.Should().NotBeNull();
            logger.Should().BeAssignableTo<ILogger<TestLoggerBuilderTests>>();
        }

        [Fact]
        public void StaticCreateCapturingLogger_ReturnsCapturingLogger()
        {
            // Act
            var logger = TestLoggerBuilder.CreateCapturingLogger<TestLoggerBuilderTests>();

            // Assert
            logger.Should().NotBeNull();
            logger.Should().BeAssignableTo<ILogger<TestLoggerBuilderTests>>();
        }

        [Fact]
        public void FluentBuilder_CanChainMultipleMethods()
        {
            // Arrange
            var expectedException = new ArgumentException("Chain test exception");

            // Act
            var logger = new TestLoggerBuilder<TestLoggerBuilderTests>()
                .WithMinLogLevel(LogLevel.Warning)
                .WithExceptionOnLog(LogLevel.Error, expectedException)
                .Build();

            // Assert
            logger.IsEnabled(LogLevel.Information).Should().BeFalse();
            logger.IsEnabled(LogLevel.Warning).Should().BeTrue();
            
            var act = () => logger.LogError("Should throw");
            act.Should().Throw<ArgumentException>()
                .WithMessage("Chain test exception");
        }
    }
}
