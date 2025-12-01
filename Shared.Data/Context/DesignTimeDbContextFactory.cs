using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Shared.Data.Context
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApiDbContext>
    {
        public ApiDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApiDbContext>();

            // This connection string is ONLY used for generating migrations.
            // It does not need to be a real database connection string.
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TempMigrationDb;Trusted_Connection=True;MultipleActiveResultSets=true");

            return new ApiDbContext(optionsBuilder.Options);
        }
    }
}
