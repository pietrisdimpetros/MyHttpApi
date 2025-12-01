using System.ComponentModel.DataAnnotations;

namespace Shared.Data.Configuration
{
    public sealed class DatabaseOptions
    {
        public const string SectionName = "Database";

        [Required]
        public required string ConnectionString { get; set; }

        [Range(0, 20)]
        public int MaxRetryCount { get; set; } = 3;

        [Range(1, 60)]
        public int MaxRetryDelaySeconds { get; set; } = 10;

        [Range(1, 300)]
        public int CommandTimeoutSeconds { get; set; } = 30;
    }
}