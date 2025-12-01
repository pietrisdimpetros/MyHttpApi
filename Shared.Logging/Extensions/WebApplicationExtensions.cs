using Microsoft.AspNetCore.Builder;
using Shared.Logging.Middleware;
namespace Shared.Logging.Extensions
{
    public static class WebApplicationExtensions
    {
        public static IApplicationBuilder UseEnterpriseLogging(this IApplicationBuilder app)
        {
            // 1. Start the Scope (Correlation ID)
            app.UseMiddleware<CorrelationMiddleware>();

            // 2. Log the Summary (Timing)
            app.UseMiddleware<RequestLoggingMiddleware>();

            return app;
        }
    }
}