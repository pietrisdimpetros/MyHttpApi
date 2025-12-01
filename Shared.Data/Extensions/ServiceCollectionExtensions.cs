using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Data.Configuration;
using Shared.Data.Context;
using Shared.Data.Entities.Identity;

namespace Shared.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Setup Database Configuration
            services.AddOptions<DatabaseOptions>()
                .Bind(configuration.GetSection(DatabaseOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // 2. Register DbContext
            services.AddDbContext<ApiDbContext>((sp, options) =>
            {
                var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;

                options.UseSqlServer(dbOptions.ConnectionString, sql =>
                {
                    sql.EnableRetryOnFailure(dbOptions.MaxRetryCount);
                    sql.CommandTimeout(dbOptions.CommandTimeoutSeconds);
                });

                // MyWebApi will register the Audit interceptor, and this line picks it up automatically.
                var interceptors = sp.GetServices<ISaveChangesInterceptor>();
                options.AddInterceptors(interceptors);
            });

            // 3. Identity Stores
            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApiDbContext>();

            return services;
        }
    }
}