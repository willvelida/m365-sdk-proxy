using M365ProxyAgent.Exceptions;
using M365ProxyAgent.Interfaces;
using M365ProxyAgent.Services;
using System.Net;
using System.Text.Json;

namespace M365ProxyAgent.Middleware
{
    /// <summary>
    /// Middleware for handling exceptions globally and providing consistent error responses.
    /// Catches all unhandled exceptions and converts them to appropriate HTTP responses.
    /// </summary>
    public class ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        ICorrelationService correlationService)
    {
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ICorrelationService _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        private static readonly string[] Body = ["Service configuration issue detected"];

        /// <summary>
        /// Processes the HTTP request and handles any exceptions that occur.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles exceptions and creates appropriate HTTP responses.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <param name="exception">The exception that occurred.</param>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var correlationId = _correlationService.CorrelationId;

            _logger.LogError(exception,
                "Unhandled exception occurred. CorrelationId: {CorrelationId}, RequestPath: {RequestPath}",
                correlationId, context.Request.Path);

            var errorResponse = CreateErrorResponse(exception, correlationId);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)errorResponse.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(errorResponse.Body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        /// <summary>
        /// Creates an appropriate error response based on the exception type.
        /// </summary>
        /// <param name="exception">The exception to create a response for.</param>
        /// <param name="correlationId">The correlation ID for the request.</param>
        /// <returns>An error response with appropriate HTTP status code and body.</returns>
        private static ErrorResponse CreateErrorResponse(Exception exception, string correlationId)
        {
            return exception switch
            {
                ValidationException validationEx => new ErrorResponse(
                    HttpStatusCode.BadRequest,
                    new ErrorBody
                    {
                        Error = "ValidationError",
                        Message = validationEx.Message,
                        CorrelationId = correlationId,
                        Details = validationEx.ValidationErrors.Length > 0
                            ? validationEx.ValidationErrors
                            : [validationEx.Message],
                        Context = validationEx.Context
                    }),

                AuthenticationException authEx => new ErrorResponse(
                    HttpStatusCode.Unauthorized,
                    new ErrorBody
                    {
                        Error = "AuthenticationError",
                        Message = "Authentication failed. Please check your credentials.",
                        CorrelationId = correlationId,
                        Details = [authEx.Message],
                        Context = authEx.Context
                    }),

                ConfigurationException configEx => new ErrorResponse(
                    HttpStatusCode.InternalServerError,
                    new ErrorBody
                    {
                        Error = "ConfigurationError",
                        Message = "A configuration error occurred. Please contact support.",
                        CorrelationId = correlationId,
                        Details = Body,
                        Context = new Dictionary<string, object> { { "ConfigurationSection", configEx.ConfigurationSection ?? "Unknown" } }
                    }),

                CopilotClientException copilotEx => new ErrorResponse(
                    HttpStatusCode.BadGateway,
                    new ErrorBody
                    {
                        Error = "CopilotServiceError",
                        Message = "Unable to communicate with Copilot Studio service.",
                        CorrelationId = correlationId,
                        Details = [copilotEx.Message],
                        Context = copilotEx.Context
                    }),

                ProxyAgentException proxyEx => new ErrorResponse(
                    HttpStatusCode.InternalServerError,
                    new ErrorBody
                    {
                        Error = "ProxyAgentError",
                        Message = proxyEx.Message,
                        CorrelationId = correlationId,
                        Details = [proxyEx.Message],
                        Context = proxyEx.Context
                    }),

                TimeoutException timeoutEx => new ErrorResponse(
                    HttpStatusCode.RequestTimeout,
                    new ErrorBody
                    {
                        Error = "TimeoutError",
                        Message = "The request timed out. Please try again.",
                        CorrelationId = correlationId,
                        Details = [timeoutEx.Message]
                    }),

                _ => new ErrorResponse(
                    HttpStatusCode.InternalServerError,
                    new ErrorBody
                    {
                        Error = "InternalServerError",
                        Message = "An unexpected error occurred. Please contact support.",
                        CorrelationId = correlationId,
                        Details = ["An internal server error occurred"]
                    })
            };
        }
    }

    /// <summary>
    /// Represents an error response with HTTP status code and body.
    /// </summary>
    /// <param name="StatusCode">The HTTP status code for the response.</param>
    /// <param name="Body">The error body containing details about the error.</param>
    public record ErrorResponse(HttpStatusCode StatusCode, ErrorBody Body);

    /// <summary>
    /// Represents the body of an error response.
    /// </summary>
    public class ErrorBody
    {
        /// <summary>
        /// Gets or sets the error type/code.
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the human-readable error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the correlation ID for tracking the request.
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets detailed error information.
        /// </summary>
        public string[] Details { get; set; } = [];

        /// <summary>
        /// Gets or sets additional context information about the error.
        /// </summary>
        public Dictionary<string, object>? Context { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the error occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
