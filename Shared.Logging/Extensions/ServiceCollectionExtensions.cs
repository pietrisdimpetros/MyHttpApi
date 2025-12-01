using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Logging.Configuration;
namespace Shared.Logging.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEnterpriseLogging(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Bind Configuration
            var options = new LoggingOptions();
            configuration.GetSection(LoggingOptions.SectionName).Bind(options);
            services.AddSingleton(options);

            // 2. Configure Logging Behavior
            services.AddLogging(builder =>
            {
                // Clear default providers (like Debug, EventSource) to reduce noise
                builder.ClearProviders();

                if (options.UseJsonFormat)
                {
                    // PRODUCTION MODE: Single-line JSON
                    builder.AddJsonConsole(json =>
                    {
                        json.IncludeScopes = true; // Crucial: This includes the CorrelationId from the Scope!
                        json.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fffZ ";
                        json.UseUtcTimestamp = true;
                    });
                }
                else
                {
                    // DEVELOPMENT MODE: Human readable
                    builder.AddSimpleConsole(simple =>
                    {
                        simple.IncludeScopes = true; // Shows [CorrelationId: ...] in brackets
                        simple.SingleLine = true;
                        simple.TimestampFormat = "HH:mm:ss ";
                    });
                }
            });

            return services;
        }
    }
}