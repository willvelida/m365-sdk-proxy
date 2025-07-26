using M365ProxyAgent.Middleware;

namespace M365ProxyAgent.Extensions
{
    /// <summary>
    /// Extension methods for registering exception handling middleware.
    /// </summary>
    public static class ExceptionHandlingExtensions
    {
        /// <summary>
        /// Adds the global exception handling middleware to the application pipeline.
        /// This middleware should be registered early in the pipeline to catch all exceptions.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }

        /// <summary>
        /// Adds exception handling services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
        {
            // Add any exception handling related services here
            // For now, the middleware only depends on services already registered
            return services;
        }
    }
}
