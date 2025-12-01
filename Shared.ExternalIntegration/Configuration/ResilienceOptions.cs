using System.ComponentModel.DataAnnotations;

namespace Shared.ExternalApiIntegration.Configuration
{
    public sealed class ResilienceOptions
    {
        [Range(1, 600)]
        public double TotalRequestTimeoutSeconds { get; set; } = 30;

        [Range(1, 10)]
        public int MaxRetryAttempts { get; set; } = 3;

        public double RetryBackoffSeconds { get; set; } = 2;
        public double CircuitBreakerFailureThreshold { get; set; } = 0.5;
        public double CircuitBreakerDurationSeconds { get; set; } = 30;
        public int RateLimiterPermitLimit { get; set; } = 100;
    }
}