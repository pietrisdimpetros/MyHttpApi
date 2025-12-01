using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Shared.Logging.Middleware
{
    public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                await next(context);
            }
            finally
            {
                sw.Stop();
                // Log the summary
                // Because we are inside CorrelationMiddleware's scope, 
                // this log will ALREADY have CorrelationId attached.
                var statusCode = context.Response.StatusCode;
                var method = context.Request.Method;
                // Define Log Level based on Status Code (Error for 500s)
                var level = statusCode >= 500 ? LogLevel.Error : LogLevel.Information;

                if (logger.IsEnabled(level))
                {
                    logger.Log(level,
                        "HTTP {Method} {Path} responded {StatusCode} in {Elapsed:0.0000} ms",
                        method,
                        context.Request.Path,
                        statusCode,
                        sw.Elapsed.TotalMilliseconds);
                }
            }
        }
    }
}