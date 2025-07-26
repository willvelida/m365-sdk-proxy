using Microsoft.Extensions.Resilience;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using M365ProxyAgent.Exceptions;
using M365ProxyAgent.Interfaces;

namespace M365ProxyAgent.Services
{
    /// <summary>
    /// Service providing resilience patterns including retry policies, circuit breakers, and timeouts.
    /// Implements Microsoft's resilience patterns for transient failure handling.
    /// </summary>
    public class ResilienceService : IResilienceService
    {
        private readonly ILogger<ResilienceService> _logger;
        private readonly ResiliencePipeline _authenticationPipeline;
        private readonly ResiliencePipeline _copilotClientPipeline;
        private readonly ResiliencePipeline _httpPipeline;

        public ResilienceService(ILogger<ResilienceService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _authenticationPipeline = CreateAuthenticationPipeline();
            _copilotClientPipeline = CreateCopilotClientPipeline();
            _httpPipeline = CreateHttpPipeline();
        }

        public ResiliencePipeline AuthenticationPipeline => _authenticationPipeline;
        public ResiliencePipeline CopilotClientPipeline => _copilotClientPipeline;
        public ResiliencePipeline HttpPipeline => _httpPipeline;

        private ResiliencePipeline CreateAuthenticationPipeline()
        {
            return new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(500),
                    MaxDelay = TimeSpan.FromSeconds(5),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder().Handle<AuthenticationException>()
                        .Handle<HttpRequestException>()
                        .Handle<TaskCanceledException>()
                        .Handle<TimeoutException>(),
                    OnRetry = args =>
                    {
                        _logger.LogWarning("Authentication operation failed, attempt {AttemptNumber} of {MaxRetryAttempts}. Exception: {Exception}",
                            args.AttemptNumber + 1, 3, args.Outcome.Exception?.Message);
                        return ValueTask.CompletedTask;
                    }
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 5,
                    BreakDuration = TimeSpan.FromSeconds(15),
                    ShouldHandle = new PredicateBuilder().Handle<AuthenticationException>(),
                    OnOpened = args =>
                    {
                        _logger.LogError("Authentication circuit breaker opened due to failures in sampling duration. Break duration: {BreakDuration}",
                            args.BreakDuration);
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = args =>
                    {
                        _logger.LogInformation("Authentication circuit breaker closed");
                        return ValueTask.CompletedTask;
                    },
                    OnHalfOpened = args =>
                    {
                        _logger.LogInformation("Authentication circuit breaker half-opened");
                        return ValueTask.CompletedTask;
                    }
                })
                .AddTimeout(TimeSpan.FromSeconds(30))
                .Build();
        }

        private ResiliencePipeline CreateCopilotClientPipeline()
        {
            return new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 2,
                    Delay = TimeSpan.FromMilliseconds(1000),
                    MaxDelay = TimeSpan.FromSeconds(10),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder().Handle<CopilotClientException>(ex =>
                        ex.HttpStatusCode >= 500 || ex.HttpStatusCode == 429)
                        .Handle<HttpRequestException>()
                        .Handle<TaskCanceledException>()
                        .Handle<TimeoutException>(),
                    OnRetry = args =>
                    {
                        _logger.LogWarning("Copilot client operation failed, attempt {AttemptNumber} of {MaxRetryAttempts}. Exception: {Exception}",
                            args.AttemptNumber + 1, 2, args.Outcome.Exception?.Message);
                        return ValueTask.CompletedTask;
                    }
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.6,
                    SamplingDuration = TimeSpan.FromSeconds(60),
                    MinimumThroughput = 3,
                    BreakDuration = TimeSpan.FromSeconds(30),
                    ShouldHandle = new PredicateBuilder().Handle<CopilotClientException>(),
                    OnOpened = args =>
                    {
                        _logger.LogError("Copilot client circuit breaker opened due to failures in sampling duration. Break duration: {BreakDuration}",
                            args.BreakDuration);
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = args =>
                    {
                        _logger.LogInformation("Copilot client circuit breaker closed");
                        return ValueTask.CompletedTask;
                    }
                })
                .AddTimeout(TimeSpan.FromSeconds(45))
                .Build();
        }

        private ResiliencePipeline CreateHttpPipeline()
        {
            return new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(200),
                    MaxDelay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Linear,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
                        .Handle<TaskCanceledException>()
                        .Handle<TimeoutException>(),
                    OnRetry = args =>
                    {
                        _logger.LogWarning("HTTP operation failed, attempt {AttemptNumber} of {MaxRetryAttempts}. Exception: {Exception}",
                            args.AttemptNumber + 1, 3, args.Outcome.Exception?.Message);
                        return ValueTask.CompletedTask;
                    }
                })
                .AddTimeout(TimeSpan.FromSeconds(10))
                .Build();
        }
    }
}
