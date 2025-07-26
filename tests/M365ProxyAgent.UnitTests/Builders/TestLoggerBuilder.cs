using Microsoft.Extensions.Logging;
using Moq;

namespace M365ProxyAgent.UnitTests.Builders
{
    public class TestLoggerBuilder<T>
    {
        private Mock<ILogger<T>> _mockLogger;
        private LogLevel _minLogLevel = LogLevel.Trace;

        public TestLoggerBuilder()
        {
            _mockLogger = new Mock<ILogger<T>>();
            SetupDefaultMockBehavior();
        }

        public TestLoggerBuilder<T> WithMinLogLevel(LogLevel minLogLevel)
        {
            _minLogLevel = minLogLevel;
            SetupIsEnabledBehavior();
            return this;
        }

        public TestLoggerBuilder<T> WithLogCapture()
        {
            var logMessages = new List<string>();
            
            _mockLogger
                .Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception?, Delegate>((level, eventId, state, exception, formatter) =>
                {
                    var message = formatter.DynamicInvoke(state, exception)?.ToString() ?? "";
                    logMessages.Add($"[{level}] {message}");
                });

            _mockLogger.Object.GetType().GetField("_capturedMessages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_mockLogger.Object, logMessages);

            return this;
        }

        public TestLoggerBuilder<T> WithExceptionOnLog(LogLevel logLevel, Exception exception)
        {
            _mockLogger
                .Setup(x => x.Log(
                    logLevel,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Throws(exception);

            return this;
        }

        public ILogger<T> Build() => _mockLogger.Object;

        public Mock<ILogger<T>> GetMock() => _mockLogger;

        public static ILogger<T> CreateLogger()
        {
            return new TestLoggerBuilder<T>().Build();
        }

        public static ILogger<T> CreateCapturingLogger()
        {
            return new TestLoggerBuilder<T>()
                .WithLogCapture()
                .Build();
        }

        public static ILogger<T> CreateLoggerWithLevel(LogLevel minLogLevel)
        {
            return new TestLoggerBuilder<T>()
                .WithMinLogLevel(minLogLevel)
                .Build();
        }

        public static ILogger<T> CreateSilentLogger()
        {
            return new TestLoggerBuilder<T>()
                .WithMinLogLevel(LogLevel.None)
                .Build();
        }

        private void SetupDefaultMockBehavior()
        {
            SetupIsEnabledBehavior();

            _mockLogger
                .Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            _mockLogger
                .Setup(x => x.BeginScope(It.IsAny<It.IsAnyType>()))
                .Returns(Mock.Of<IDisposable>());
        }

        private void SetupIsEnabledBehavior()
        {
            _mockLogger
                .Setup(x => x.IsEnabled(It.IsAny<LogLevel>()))
                .Returns<LogLevel>(level => level >= _minLogLevel && _minLogLevel != LogLevel.None);
        }
    }

    public static class TestLoggerBuilder
    {
        public static TestLoggerBuilder<T> For<T>()
        {
            return new TestLoggerBuilder<T>();
        }

        public static ILogger<T> CreateLogger<T>()
        {
            return TestLoggerBuilder<T>.CreateLogger();
        }

        public static ILogger<T> CreateCapturingLogger<T>()
        {
            return TestLoggerBuilder<T>.CreateCapturingLogger();
        }
    }
}
