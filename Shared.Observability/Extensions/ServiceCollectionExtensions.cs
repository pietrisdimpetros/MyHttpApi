using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Observability.Configuration;
namespace Shared.Observability.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureObservability(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Bind & Validate Options
            services.AddOptions<AzureObservabilityOptions>()
                .Bind(configuration.GetSection(AzureObservabilityOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var options = configuration.GetSection(AzureObservabilityOptions.SectionName).Get<AzureObservabilityOptions>();

            if (options is null || string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                return services;
            }

            // 2. Configure OpenTelemetry with Azure Monitor
            // This "One Liner" sets up Tracing, Metrics, and Logging automatically.
            // It uses the native "SqlClient" instrumentation which IS included in the stable release.
            services.AddOpenTelemetry()
                .UseAzureMonitor(o =>
                {
                    o.ConnectionString = options.ConnectionString;
                    o.SamplingRatio = options.SamplingRatio;
                });

            return services;
        }
    }
}