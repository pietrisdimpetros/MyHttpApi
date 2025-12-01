using System.ComponentModel.DataAnnotations;
namespace Shared.Documentation.Configuration
{
    public sealed class DocumentationOptions
    {
        public const string SectionName = "Documentation";

        [Required]
        public required string Title { get; set; } = "Enterprise API";

        [Required]
        public required string Version { get; set; } = "v1";

        public string? Description { get; set; }

        // Azure AD Settings
        [Required]
        public required string TenantId { get; set; }

        [Required]
        public required string ClientId { get; set; }

        // The scope required to access this API (e.g. "api://<client-id>/access_as_user")
        [Required]
        public required string[] Scopes { get; set; }

        public bool EnableXmlComments { get; set; } = true;
    }
}