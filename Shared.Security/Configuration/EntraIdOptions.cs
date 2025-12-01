using System.ComponentModel.DataAnnotations;
namespace Shared.Security.Configuration
{
    public sealed class EntraIdOptions
    {
        public const string SectionName = "AzureAd";

        [Required]
        public required string Instance { get; set; } = "https://login.microsoftonline.com/";

        [Required]
        public required string Domain { get; set; }

        [Required]
        public required string TenantId { get; set; }

        [Required]
        public required string ClientId { get; set; }

        // Optional: If you want to enforce a specific claim type for Roles
        public string RoleClaimType { get; set; } = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
    }
}