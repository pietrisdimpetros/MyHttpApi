using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using Polly.RateLimiting;

namespace MyHttpApi.Infrastructure
{
    public sealed class ExternalServiceExceptionHandler(ILogger<ExternalServiceExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Default to 500
            var status = StatusCodes.Status500InternalServerError;
            var title = "An error occurred";
            var detail = exception.Message;

            // Map specific failures to specific HTTP Codes
            switch (exception)
            {
                // 1. Timeout (Polly or HttpClient)
                case TaskCanceledException:
                case TimeoutException:
                    status = StatusCodes.Status504GatewayTimeout;
                    title = "Upstream Service Timeout";
                    detail = "The external service did not respond in time.";
                    break;

                // 2. Circuit Breaker Open
                case BrokenCircuitException:
                    status = StatusCodes.Status503ServiceUnavailable;
                    title = "Service Degraded";
                    detail = "The external service is currently down. Please try again later.";
                    context.Response.Headers.RetryAfter = "30"; // Hint to client
                    break;

                // 3. Rate Limit Exceeded (Polly)
                case RateLimiterRejectedException:
                    status = StatusCodes.Status429TooManyRequests;
                    title = "Rate Limit Exceeded";
                    detail = "We are sending too many requests to the upstream provider.";
                    break;

                // 4. General HTTP Failure
                case HttpRequestException:
                    status = StatusCodes.Status502BadGateway;
                    title = "Upstream Service Error";
                    detail = "Received an invalid response from the external provider.";
                    break;
            }

            //log only if logging is enabled 
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(exception, "Handled external service exception: {Title} - {Detail}", title, detail);


            var problemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            // Add standardized error
            problemDetails.Extensions.Add("traceId", context.TraceIdentifier);

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}