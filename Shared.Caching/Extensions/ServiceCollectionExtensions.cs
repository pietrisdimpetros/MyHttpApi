using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Caching.Configuration;
using Shared.Caching.Interfaces;
using Shared.Caching.Services;

namespace Shared.Caching.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCachingInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Validate Options
            services.AddOptions<CacheOptions>()
                .Bind(configuration.GetSection(CacheOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // 2. Register Native Memory Cache
            services.AddMemoryCache();

            // 3. Register our "Strict" Agent
            // Even though it uses MemoryCache, it enforces JSON serialization and Prefixes☺
            services.AddSingleton<ICacheAgent, InMemoryCacheAgent>();

            return services;
        }
    }
}
