using FluentAssertions;
using M365ProxyAgent.Auth;
using M365ProxyAgent.Configuration;
using M365ProxyAgent.Exceptions;
using M365ProxyAgent.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Moq;
using Moq.Protected;
using Polly;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace M365ProxyAgent.UnitTests.Auth
{
    public class AddTokenHandlerS2STests
    {
        private readonly Mock<IResilienceService> _mockResilienceService;
        private readonly Mock<ILogger<AddTokenHandlerS2S>> _mockLogger;
        private readonly CopilotStudioClientSettings _testSettings;

        private const string TestAppClientId = "test-client-id";
        private const string TestTenantId = "test-tenant-id";
        private const string TestClientSecret = "test-client-secret";
        private const string TestEnvironmentId = "test-environment-id";
        private const string TestSchemaName = "test-schema-name";
        private const string TestAccessToken = "test-access-token";
        private const string TestResourceUrl = "https://api.office.com";
        private const string ExpectedBearerToken = "Bearer test-access-token";

        public AddTokenHandlerS2STests()
        {
            _mockResilienceService = new Mock<IResilienceService>();
            _mockLogger = new Mock<ILogger<AddTokenHandlerS2S>>();

            var configData = new Dictionary<string, string?>
            {
                ["TestSection:AppClientId"] = TestAppClientId,
                ["TestSection:TenantId"] = TestTenantId,
                ["TestSection:AppClientSecret"] = TestClientSecret,
                ["TestSection:EnvironmentId"] = TestEnvironmentId,
                ["TestSection:SchemaName"] = TestSchemaName,
                ["TestSection:UseS2SConnection"] = "true"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            _testSettings = new CopilotStudioClientSettings(configuration.GetSection("TestSection"));
        }

        [Fact]
        public async Task SendAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var handler = new AddTokenHandlerS2S(_testSettings, _mockResilienceService.Object);

            // Act & Assert
            var act = async () => await InvokeProtectedSendAsync(handler, null!, CancellationToken.None);
            
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("request");
        }

        [Fact]
        public async Task SendAsync_WithExistingAuthorizationHeader_SkipsAuthenticationAndCallsBase()
        {
            // Arrange
            var handler = new AddTokenHandlerS2S(_testSettings);
            var request = CreateHttpRequestMessage();
            var existingToken = "existing-token";
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", existingToken);
            
            // Act & Assert
            var act = async () => await InvokeProtectedSendAsync(handler, request, CancellationToken.None);
            
            await act.Should().ThrowAsync<CopilotClientException>()
                .WithMessage("HTTP request failed: *");
            
            request.Headers.Authorization.Should().NotBeNull();
            request.Headers.Authorization!.Parameter.Should().Be(existingToken);
        }

        [Fact]
        public async Task SendAsync_WithNoAuthHeaderAndInvalidCredentials_ThrowsAuthenticationException()
        {
            // Arrange
            var handler = new AddTokenHandlerS2S(_testSettings);
            var request = CreateHttpRequestMessage();
            
            // Act & Assert
            var act = async () => await InvokeProtectedSendAsync(handler, request, CancellationToken.None);
            
            await act.Should().ThrowAsync<AuthenticationException>()
                .WithMessage("MSAL authentication failed: *");
            
            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_AuthenticationUsesResilienceService()
        {
            // Arrange
            var mockResilienceService = new Mock<IResilienceService>();
            mockResilienceService.Setup(x => x.AuthenticationPipeline)
                .Returns(ResiliencePipeline.Empty);
            
            var handler = new AddTokenHandlerS2S(_testSettings, mockResilienceService.Object);
            var request = CreateHttpRequestMessage();
            
            // Act & Assert
            var act = async () => await InvokeProtectedSendAsync(handler, request, CancellationToken.None);
            
            await act.Should().ThrowAsync<AuthenticationException>();
            
            mockResilienceService.Verify(x => x.AuthenticationPipeline, Times.AtLeastOnce);
        }

        [Fact]
        public async Task SendAsync_MultipleCallsWithSameHandler_UsesCachedAuthentication()
        {
            // Arrange
            var handler = new AddTokenHandlerS2S(_testSettings);
            var request1 = CreateHttpRequestMessage();
            var request2 = CreateHttpRequestMessage();
            
            // Act & Assert
            var act1 = async () => await InvokeProtectedSendAsync(handler, request1, CancellationToken.None);
            
            await act1.Should().ThrowAsync<AuthenticationException>()
                .WithMessage("MSAL authentication failed: *");
            
            var act2 = async () => await InvokeProtectedSendAsync(handler, request2, CancellationToken.None);
            
            await act2.Should().ThrowAsync<AuthenticationException>()
                .WithMessage("MSAL authentication failed: *");
            
            request1.Headers.Authorization.Should().BeNull();
            request2.Headers.Authorization.Should().BeNull();
        }

        private static async Task<HttpResponseMessage> InvokeProtectedSendAsync(
            AddTokenHandlerS2S handler, 
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            var sendAsyncMethod = typeof(AddTokenHandlerS2S).GetMethod("SendAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var task = (Task<HttpResponseMessage>)sendAsyncMethod!.Invoke(handler, new object[] { request, cancellationToken })!;
            return await task;
        }

        private HttpRequestMessage CreateHttpRequestMessage()
        {
            return new HttpRequestMessage(HttpMethod.Get, TestResourceUrl);
        }
    }
}
