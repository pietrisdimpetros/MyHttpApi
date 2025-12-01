using Microsoft.AspNetCore.Identity;

namespace Shared.Data.Entities.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string? FirstName { get; set; }

        [PersonalData]
        public string? LastName { get; set; }

        // Example of audit property
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}