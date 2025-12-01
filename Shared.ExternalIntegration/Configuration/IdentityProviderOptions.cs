using System.ComponentModel.DataAnnotations;

namespace Shared.ExternalApiIntegration.Configuration
{
    public sealed class IdentityProviderOptions
    {
        public const string SectionName = "ExternalApi:IdentityProvider";

        [Required, Url]
        public required string TokenUrl { get; set; } // e.g. https://login.microsoftonline.com/.../token

        [Required]
        public required string ClientId { get; set; }

        [Required]
        public required string ClientSecret { get; set; }

        [Required]
        public required string Scope { get; set; } = ".default";
    }
}
