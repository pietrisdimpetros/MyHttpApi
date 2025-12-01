using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Shared.Security.Services
{
    public class ClaimsAugmentationService<TUser>(
      UserManager<TUser> userManager,
      IMemoryCache cache,
      ILogger<ClaimsAugmentationService<TUser>> logger)
      where TUser : class
    {
        public async Task EnrichPrincipalAsync(ClaimsPrincipal principal)
        {
            // 1. Extract Entra ID (OID - Object ID)
            // Azure AD usually stores the unique ID in "http://schemas.microsoft.com/identity/claims/objectidentifier" 
            // or just "oid" depending on version.
            var entraId = principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                          ?? principal.FindFirst("oid")?.Value;

            if (string.IsNullOrEmpty(entraId))
            {
                logger.LogWarning("Token validated but no Entra ID (oid) claim found.");
                return;
            }

            // 2. Lookup User (Cache Look-aside)
            // We assume the local AspNetUsers 'Id' column matches the Azure 'oid'
            // If your mapping is different (e.g. Email), change FindByIdAsync to FindByEmailAsync
            var roles = await cache.GetOrCreateAsync($"roles_{entraId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10); // Cache roles for 10 mins

                var user = await userManager.FindByIdAsync(entraId);

                if (user is null)
                {
                    logger.LogWarning("User with Entra ID {EntraId} not found in AspNetUsers.", entraId);
                    return Array.Empty<string>();
                }

                // 3. Get Roles from AspNetUserRoles table
                var userRoles = await userManager.GetRolesAsync(user);
                return userRoles;
            });

            if (roles is not null && roles.Count != 0)
            {
                // 4. Attach Roles to Principal
                var claimsIdentity = (ClaimsIdentity)principal.Identity!;

                foreach (var role in roles)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                }

                logger.LogDebug("Augmented user {EntraId} with roles: {Roles}", entraId, string.Join(",", roles));
            }
        }
    }
}
