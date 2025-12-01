using System.ComponentModel.DataAnnotations;

namespace Shared.ExternalApiIntegration.Configuration
{
    public sealed class ExternalApiOptions
    {
        public const string SectionName = "ExternalApi";

        [Required, Url]
        public required string BaseUrl { get; set; }

        [Required]
        public required string OcpApimSubscriptionKey { get; set; }

        [Required]
        public ResilienceOptions Resilience { get; set; } = new();

    }
}
