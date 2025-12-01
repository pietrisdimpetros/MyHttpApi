using System.ComponentModel.DataAnnotations;

namespace Shared.Observability.Configuration
{
    public sealed class AzureObservabilityOptions
    {
        public const string SectionName = "AzureMonitor";

        [Required]
        public required string ConnectionString { get; set; }

        /// <summary>
        /// Sampling ratio (0.0 to 1.0). 
        /// 1.0 = Record 100% of requests.
        /// 0.1 = Record 10% of requests (Save money in Prod).
        /// </summary>
        [Range(0.0, 1.0)]
        public float SamplingRatio { get; set; } = 1.0f;
    }
}