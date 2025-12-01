using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
namespace Shared.Logging.Middleware
{
    public sealed class CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Get or Generate ID
            string correlationId = GetCorrelationId(context);

            // 2. Add to Response Headers (for client tracking)
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
            }

            // 3. Create the Logging Scope
            // Anything logged inside this scope (Controllers, EF Core, etc.)
            // will automatically inherit "CorrelationId" and "RequestPath".
            var scopeState = new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["RequestPath"] = context.Request.Path.Value ?? "/"
            };

            // If user is already authenticated by previous middleware, attach that too
            if (context.User.Identity?.IsAuthenticated == true)
            {
                scopeState["User"] = context.User.Identity.Name ?? "Anonymous";
            }

            using (logger.BeginScope(scopeState))
            {
                await next(context);
            }
        }

        private static string GetCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var value))
            {
                return value.ToString();
            }
            return Guid.NewGuid().ToString();
        }
    }
}