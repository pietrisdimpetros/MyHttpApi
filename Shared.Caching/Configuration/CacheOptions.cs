using System.ComponentModel.DataAnnotations;

namespace Shared.Caching.Configuration
{
    public sealed class CacheOptions
    {
        public const string SectionName = "Caching";

        [Required]
        public required string InstanceName { get; set; } = "Local:";

        [Range(1, 1440)]
        public int DefaultExpirationMinutes { get; set; } = 30;
    }
}