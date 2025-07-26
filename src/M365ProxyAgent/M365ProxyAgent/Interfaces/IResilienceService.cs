using Polly;

namespace M365ProxyAgent.Interfaces
{
    /// <summary>
    /// Interface for resilience operations including retry policies and circuit breakers.
    /// </summary>
    public interface IResilienceService
    {
        /// <summary>
        /// Gets the resilience pipeline for authentication operations.
        /// </summary>
        ResiliencePipeline AuthenticationPipeline { get; }

        /// <summary>
        /// Gets the resilience pipeline for Copilot client operations.
        /// </summary>
        ResiliencePipeline CopilotClientPipeline { get; }

        /// <summary>
        /// Gets the resilience pipeline for HTTP operations.
        /// </summary>
        ResiliencePipeline HttpPipeline { get; }
    }
}
