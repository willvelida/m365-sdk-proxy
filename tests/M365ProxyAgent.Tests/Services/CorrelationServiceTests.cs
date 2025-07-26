using FluentAssertions;
using M365ProxyAgent.Services;

namespace M365ProxyAgent.Tests.Services
{
    public class CorrelationServiceTests
    {
        private const string TestCorrelationId = "test-correlation-id-12345";
        private const string NewTestCorrelationId = "new-test-correlation-id-67890";
        private const int ExpectedGuidLength = 36;
        private const string GuidRegexPattern = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
        
        private readonly CorrelationService _correlationService;

        public CorrelationServiceTests()
        {
            _correlationService = new CorrelationService();
        }

        [Fact]
        public void GenerateCorrelationId_WhenCalled_ReturnsValidGuid()
        {
            // Act
            var result = _correlationService.GenerateCorrelationId();

            // Assert
            ValidateGuidFormat(result);
        }

        [Fact]
        public void CorrelationId_WhenNoIdExists_GeneratesNewGuid()
        {
            // Act
            var result = _correlationService.CorrelationId;

            // Assert
            ValidateGuidFormat(result);
            
            var secondCall = _correlationService.CorrelationId;
            secondCall.Should().Be(result, "correlation ID should be cached after first generation");
        }

        [Fact]
        public void SetCorrelationId_WithValidId_SetsCorrelationId()
        {
            // Act
            _correlationService.SetCorrelationId(TestCorrelationId);

            // Assert
            _correlationService.CorrelationId.Should().Be(TestCorrelationId);
        }

        [Fact]
        public void SetCorrelationId_WithNullId_ThrowsArgumentNullException()
        {
            // Act
            var act = () => _correlationService.SetCorrelationId(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("correlationId");
        }

        [Fact]
        public void SetCorrelationId_WithEmptyId_SetsEmptyCorrelationId()
        {
            // Act
            _correlationService.SetCorrelationId(string.Empty);

            // Assert
            _correlationService.CorrelationId.Should().Be(string.Empty);
        }

        [Fact]
        public void SetCorrelationId_WithWhitespaceId_SetsWhitespaceCorrelationId()
        {
            // Arrange
            const string whitespaceId = "   ";

            // Act
            _correlationService.SetCorrelationId(whitespaceId);

            // Assert
            _correlationService.CorrelationId.Should().Be(whitespaceId);
        }

        [Fact]
        public void SetCorrelationId_CalledMultipleTimes_UpdatesCorrelationId()
        {
            // Arrange
            _correlationService.SetCorrelationId(TestCorrelationId);
            _correlationService.CorrelationId.Should().Be(TestCorrelationId);

            // Act
            _correlationService.SetCorrelationId(NewTestCorrelationId);

            // Assert
            _correlationService.CorrelationId.Should().Be(NewTestCorrelationId);
        }

        [Fact]
        public void CorrelationId_AfterSettingCustomId_ReturnsSetValue()
        {
            // Arrange
            _correlationService.SetCorrelationId(TestCorrelationId);

            // Act
            var result = _correlationService.CorrelationId;

            // Assert
            result.Should().Be(TestCorrelationId);
        }

        [Fact]
        public void GenerateCorrelationId_CalledMultipleTimes_ReturnsUniqueValues()
        {
            // Act
            var firstId = _correlationService.GenerateCorrelationId();
            var secondId = _correlationService.GenerateCorrelationId();

            // Assert
            ValidateGuidFormat(firstId);
            ValidateGuidFormat(secondId);
            firstId.Should().NotBe(secondId, "each call should generate a unique correlation ID");
        }

        private static void ValidateGuidFormat(string guidString)
        {
            guidString.Should().NotBeNullOrEmpty();
            Guid.TryParse(guidString, out _).Should().BeTrue("correlation ID should be a valid GUID format");
            guidString.Should().HaveLength(ExpectedGuidLength, "GUID string should be 36 characters in format 'D'");
            guidString.Should().MatchRegex(GuidRegexPattern, 
                "correlation ID should match standard GUID format");
        }
    }
}
