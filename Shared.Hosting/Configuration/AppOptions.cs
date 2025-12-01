using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Shared.Hosting.Configuration
{
    public sealed class AppOptions
    {
        public const string SectionName = "App";

        [Required]
        public required string Name { get; set; } // Used for Logs, Metrics, Swagger Title

        [Required]
        public required string Version { get; set; } = "v1";

        // Identity settings shared by Security and Documentation
        [Required]
        public required string TenantId { get; set; }

        [Required]
        public required string ClientId { get; set; }

        public string[] AllowedOrigins { get; set; } = [];

        [Required]
        public required string Domain { get; set; } // e.g. "mycompany.onmicrosoft.com"
    }
}
