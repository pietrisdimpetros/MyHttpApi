using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Shared.ExternalApiIntegration.Authentication;
using Shared.ExternalApiIntegration.Clients;
using Shared.ExternalApiIntegration.Configuration;
using Shared.ExternalApiIntegration.Handlers;
using Shared.ExternalApiIntegration.Interfaces;
using System.Threading.RateLimiting;

namespace Shared.ExternalApiIntegration.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExternalApiIntegration(this IServiceCollection services)
        {
            // 1. Identity Provider Configuration
            services.AddOptions<IdentityProviderOptions>()
                  .BindConfiguration(IdentityProviderOptions.SectionName)
                  .ValidateDataAnnotations()
                  .ValidateOnStart();

            // 2. Register Token Fetcher
            services.AddTransient<ITokenFetcher, OidcTokenFetcher>();

            // 3. Identity Provider HTTP Client (With Resilience)
            services.AddHttpClient("IdPClient")
            .AddResilienceHandler("idp-resilience", (pipeline, context) =>
            {
                // FIX: Use RetryStrategyOptions<HttpResponseMessage>
                pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Constant,
                    Delay = TimeSpan.FromSeconds(1),
                    // FIX: Use PredicateBuilder<HttpResponseMessage>
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .HandleResult(r => !r.IsSuccessStatusCode) // Also retry on non-success codes
                });

                pipeline.AddTimeout(TimeSpan.FromSeconds(5));
            });

            // 4. External API Configuration
            services.AddOptions<ExternalApiOptions>()
                .BindConfiguration(ExternalApiOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // 5. Shared Services
            services.AddMemoryCache();
            services.AddTransient<AuthHeaderHandler>();

            // 6. External API Client (With Complex Resilience)
            services.AddHttpClient<IExternalApiClient, ExternalApiClient>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<ExternalApiOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>()
            .AddResilienceHandler("external-resilience", (pipeline, context) =>
            {
                var resilienceOps = context.ServiceProvider
                    .GetRequiredService<IOptions<ExternalApiOptions>>()
                    .Value.Resilience;

                // A. Total Timeout
                pipeline.AddTimeout(TimeSpan.FromSeconds(resilienceOps.TotalRequestTimeoutSeconds));

                // B. Rate Limiting (Note: RateLimiters are protocol agnostic, so this remains the same)
                pipeline.AddRateLimiter(new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = resilienceOps.RateLimiterPermitLimit,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

                // C. Circuit Breaker
                pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
                {
                    FailureRatio = resilienceOps.CircuitBreakerFailureThreshold,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 5,
                    BreakDuration = TimeSpan.FromSeconds(resilienceOps.CircuitBreakerDurationSeconds),
                    // FIX: Use PredicateBuilder<HttpResponseMessage>
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .HandleResult(r => (int)r.StatusCode >= 500) // Trip breaker on 5xx
                });

                // D. Retry
                pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    MaxRetryAttempts = resilienceOps.MaxRetryAttempts,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromSeconds(resilienceOps.RetryBackoffSeconds),
                    UseJitter = true,
                    // FIX: Use PredicateBuilder<HttpResponseMessage>
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .HandleResult(r => (int)r.StatusCode >= 500) // Retry on 5xx
                });
            });

            return services;
        }
    }
}