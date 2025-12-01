using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Entities;
using Shared.Data.Entities.Identity;

namespace Shared.Data.Context
{
    public class ApiDbContext(DbContextOptions<ApiDbContext> options)
     : IdentityDbContext<ApplicationUser>(options)
    {
        // Public DbSet<YourEntity> YourEntities { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize Identity Table names if preferred
            // builder.Entity<ApplicationUser>().ToTable("Users");
            // builder.Entity<IdentityRole>().ToTable("Roles");

            // Apply Configurations from the Entities folder (future proofing)
            builder.ApplyConfigurationsFromAssembly(typeof(ApiDbContext).Assembly);
        }
    }
}
