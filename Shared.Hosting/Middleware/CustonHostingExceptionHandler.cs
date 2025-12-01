using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
namespace Shared.Hosting.Middleware
{
    public sealed class EnterpriseExceptionHandler(ILogger<EnterpriseExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var traceId = context.TraceIdentifier;

            // Log the full error with the Trace ID so you can find it in Azure/Splunk
            logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred while processing your request.",
                Detail = "Please refer to the Trace ID for support.",
                Instance = context.Request.Path,
                Extensions = { ["traceId"] = traceId }
            };

            context.Response.StatusCode = problemDetails.Status.Value;
            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}